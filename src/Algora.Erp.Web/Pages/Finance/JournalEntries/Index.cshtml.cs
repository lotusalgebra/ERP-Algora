using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.JournalEntries;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int DraftEntries { get; set; }
    public int PendingEntries { get; set; }
    public int PostedEntries { get; set; }
    public decimal TotalThisMonth { get; set; }

    public async Task OnGetAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        DraftEntries = await _context.JournalEntries.CountAsync(j => j.Status == JournalEntryStatus.Draft);
        PendingEntries = await _context.JournalEntries.CountAsync(j => j.Status == JournalEntryStatus.Pending);
        PostedEntries = await _context.JournalEntries.CountAsync(j => j.Status == JournalEntryStatus.Posted);
        TotalThisMonth = await _context.JournalEntries
            .Where(j => j.EntryDate >= startOfMonth && j.Status == JournalEntryStatus.Posted)
            .SumAsync(j => j.TotalDebit);
    }

    public async Task<IActionResult> OnGetTableAsync(
        string? search,
        string? statusFilter,
        string? typeFilter,
        string? dateFrom,
        string? dateTo,
        int page = 1,
        int pageSize = 15)
    {
        var query = _context.JournalEntries
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(j =>
                j.EntryNumber.ToLower().Contains(search) ||
                j.Description.ToLower().Contains(search) ||
                (j.Reference != null && j.Reference.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && int.TryParse(statusFilter, out var status))
        {
            query = query.Where(j => (int)j.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && int.TryParse(typeFilter, out var type))
        {
            query = query.Where(j => (int)j.EntryType == type);
        }

        if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var fromDate))
        {
            query = query.Where(j => j.EntryDate >= fromDate);
        }

        if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var toDate))
        {
            query = query.Where(j => j.EntryDate <= toDate);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var entries = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.EntryNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_JournalEntriesTableRows", new JournalEntriesTableViewModel
        {
            Entries = entries,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnPostPostAsync(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
            return NotFound();

        if (entry.TotalDebit != entry.TotalCredit)
        {
            return BadRequest("Entry is not balanced. Debit must equal Credit.");
        }

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedAt = DateTime.UtcNow;

        // Update account balances
        foreach (var line in entry.Lines)
        {
            var account = await _context.Accounts.FindAsync(line.AccountId);
            if (account != null)
            {
                // Assets and Expenses increase with Debit
                // Liabilities, Equity, and Revenue increase with Credit
                if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
                {
                    account.CurrentBalance += line.DebitAmount - line.CreditAmount;
                }
                else
                {
                    account.CurrentBalance += line.CreditAmount - line.DebitAmount;
                }
            }
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null)
            return NotFound();

        if (entry.Status == JournalEntryStatus.Posted)
        {
            return BadRequest("Cannot delete posted entries. Reverse instead.");
        }

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null, null);
    }
}

public class JournalEntriesTableViewModel
{
    public List<JournalEntry> Entries { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
