using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.JournalEntries;

public class EditModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public EditModel(IApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public JournalEntryEditInput Input { get; set; } = new();

    public JournalEntry Entry { get; set; } = null!;
    public List<SelectListItem> Accounts { get; set; } = new();
    public List<SelectListItem> EntryTypes { get; set; } = new();
    public bool IsReadOnly { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
            return NotFound();

        Entry = entry;
        IsReadOnly = entry.Status == JournalEntryStatus.Posted ||
                     entry.Status == JournalEntryStatus.Reversed ||
                     entry.Status == JournalEntryStatus.Void;

        Input = new JournalEntryEditInput
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            Reference = entry.Reference,
            EntryType = entry.EntryType,
            Notes = entry.Notes,
            IsAdjusting = entry.IsAdjusting,
            Status = entry.Status,
            Lines = entry.Lines.OrderBy(l => l.LineNumber).Select(l => new JournalEntryLineEditInput
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountName = $"{l.Account.Code} - {l.Account.Name}",
                Description = l.Description,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount
            }).ToList()
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == Input.Id);

        if (entry == null)
            return NotFound();

        // Can't edit posted/reversed/void entries
        if (entry.Status == JournalEntryStatus.Posted ||
            entry.Status == JournalEntryStatus.Reversed ||
            entry.Status == JournalEntryStatus.Void)
        {
            TempData["ErrorMessage"] = "Cannot edit a posted, reversed, or voided entry.";
            return RedirectToPage("Edit", new { id = Input.Id });
        }

        // Remove empty lines
        Input.Lines = Input.Lines
            .Where(l => l.AccountId != Guid.Empty && (l.DebitAmount > 0 || l.CreditAmount > 0))
            .ToList();

        if (Input.Lines.Count < 2)
        {
            ModelState.AddModelError("", "A journal entry must have at least 2 lines.");
            Entry = entry;
            await LoadDropdownsAsync();
            return Page();
        }

        var totalDebit = Input.Lines.Sum(l => l.DebitAmount);
        var totalCredit = Input.Lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
        {
            ModelState.AddModelError("", $"Entry is not balanced. Debit ({totalDebit:C}) must equal Credit ({totalCredit:C}).");
            Entry = entry;
            await LoadDropdownsAsync();
            return Page();
        }

        // Update entry
        entry.EntryDate = Input.EntryDate;
        entry.Description = Input.Description;
        entry.Reference = Input.Reference;
        entry.EntryType = Input.EntryType;
        entry.Notes = Input.Notes;
        entry.IsAdjusting = Input.IsAdjusting;
        entry.TotalDebit = totalDebit;
        entry.TotalCredit = totalCredit;

        if (Input.SubmitForApproval && entry.Status == JournalEntryStatus.Draft)
        {
            entry.Status = JournalEntryStatus.Pending;
        }

        // Remove existing lines and add new ones
        _context.JournalEntryLines.RemoveRange(entry.Lines);

        int lineNumber = 1;
        foreach (var line in Input.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = line.AccountId,
                Description = line.Description,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                LineNumber = lineNumber++
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Journal Entry {entry.EntryNumber} updated successfully.";
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
            return NotFound();

        if (entry.Status == JournalEntryStatus.Posted)
        {
            TempData["ErrorMessage"] = "Cannot delete a posted entry. Reverse it instead.";
            return RedirectToPage("Edit", new { id });
        }

        _context.JournalEntryLines.RemoveRange(entry.Lines);
        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Journal Entry {entry.EntryNumber} deleted.";
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

        // Ensure at least 2 lines
        while (Input.Lines.Count < 2)
        {
            Input.Lines.Add(new JournalEntryLineEditInput());
        }
    }
}

public class JournalEntryEditInput
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public JournalEntryType EntryType { get; set; }
    public string? Notes { get; set; }
    public bool IsAdjusting { get; set; }
    public JournalEntryStatus Status { get; set; }
    public bool SubmitForApproval { get; set; }
    public List<JournalEntryLineEditInput> Lines { get; set; } = new();
}

public class JournalEntryLineEditInput
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}
