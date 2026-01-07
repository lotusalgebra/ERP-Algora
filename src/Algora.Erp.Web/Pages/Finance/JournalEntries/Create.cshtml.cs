using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.JournalEntries;

public class CreateModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public CreateModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public JournalEntryInput Input { get; set; } = new();

    public List<SelectListItem> Accounts { get; set; } = new();
    public List<SelectListItem> EntryTypes { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove validation for lines that are empty
        Input.Lines = Input.Lines
            .Where(l => l.AccountId != Guid.Empty && (l.DebitAmount > 0 || l.CreditAmount > 0))
            .ToList();

        if (Input.Lines.Count < 2)
        {
            ModelState.AddModelError("", "A journal entry must have at least 2 lines.");
            await LoadDropdownsAsync();
            return Page();
        }

        var totalDebit = Input.Lines.Sum(l => l.DebitAmount);
        var totalCredit = Input.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
        {
            ModelState.AddModelError("", $"Entry is not balanced. Debit ({totalDebit:C}) must equal Credit ({totalCredit:C}).");
            await LoadDropdownsAsync();
            return Page();
        }

        // Generate entry number
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
        var lastEntry = await _context.JournalEntries
            .Where(j => j.EntryNumber.StartsWith($"JE{yearMonth}"))
            .OrderByDescending(j => j.EntryNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastEntry != null)
        {
            var lastNumberStr = lastEntry.EntryNumber.Replace($"JE{yearMonth}", "");
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        var entryNumber = $"JE{yearMonth}{nextNumber:D3}";

        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            EntryDate = Input.EntryDate,
            Description = Input.Description,
            Reference = Input.Reference,
            EntryType = Input.EntryType,
            Notes = Input.Notes,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Status = Input.SaveAsDraft ? JournalEntryStatus.Draft : JournalEntryStatus.Pending,
            IsAdjusting = Input.IsAdjusting
        };

        int lineNumber = 1;
        foreach (var line in Input.Lines)
        {
            journalEntry.Lines.Add(new JournalEntryLine
            {
                AccountId = line.AccountId,
                Description = line.Description,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                LineNumber = lineNumber++
            });
        }

        _context.JournalEntries.Add(journalEntry);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Journal Entry {entryNumber} created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var accounts = await _context.Accounts
            .Where(a => a.IsActive && a.AllowDirectPosting)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.Code)
            .Select(a => new { a.Id, a.Code, a.Name, a.AccountType })
            .ToListAsync();

        Accounts = accounts
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Code} - {a.Name}",
                Group = new SelectListGroup { Name = a.AccountType.ToString() }
            })
            .ToList();

        EntryTypes = Enum.GetValues<JournalEntryType>()
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToString()
            })
            .ToList();

        // Ensure at least 2 empty lines for new entries
        if (Input.Lines.Count == 0)
        {
            Input.Lines = new List<JournalEntryLineInput>
            {
                new(),
                new()
            };
        }
    }
}

public class JournalEntryInput
{
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public JournalEntryType EntryType { get; set; } = JournalEntryType.General;
    public string? Notes { get; set; }
    public bool IsAdjusting { get; set; }
    public bool SaveAsDraft { get; set; } = true;
    public List<JournalEntryLineInput> Lines { get; set; } = new();
}

public class JournalEntryLineInput
{
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
