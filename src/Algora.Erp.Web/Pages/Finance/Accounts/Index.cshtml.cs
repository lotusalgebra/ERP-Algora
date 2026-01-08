using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FinanceAccount = Algora.Erp.Domain.Entities.Finance.Account;

namespace Algora.Erp.Web.Pages.Finance.Accounts;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal NetIncome { get; set; }

    public async Task OnGetAsync()
    {
        TotalAssets = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Asset && a.IsActive)
            .SumAsync(a => a.CurrentBalance);

        TotalLiabilities = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Liability && a.IsActive)
            .SumAsync(a => a.CurrentBalance);

        TotalEquity = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Equity && a.IsActive)
            .SumAsync(a => a.CurrentBalance);

        var totalRevenue = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Revenue && a.IsActive)
            .SumAsync(a => a.CurrentBalance);

        var totalExpenses = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Expense && a.IsActive)
            .SumAsync(a => a.CurrentBalance);

        NetIncome = totalRevenue - totalExpenses;
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? typeFilter, string? statusFilter, int page = 1, int pageSize = 15)
    {
        var query = _context.Accounts
            .Include(a => a.ParentAccount)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(a =>
                a.Code.ToLower().Contains(search) ||
                a.Name.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && int.TryParse(typeFilter, out var type))
        {
            query = query.Where(a => (int)a.AccountType == type);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(a => a.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var accounts = await query
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_AccountsTableRows", new AccountsTableViewModel
        {
            Accounts = accounts,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var parentAccounts = await _context.Accounts
            .Where(a => a.IsActive && a.AllowDirectPosting)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.Code)
            .ToListAsync();

        return Partial("_AccountForm", new AccountFormViewModel
        {
            IsEdit = false,
            ParentAccounts = parentAccounts
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var account = await _context.Accounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
            return NotFound();

        var parentAccounts = await _context.Accounts
            .Where(a => a.IsActive && a.Id != id)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.Code)
            .ToListAsync();

        return Partial("_AccountForm", new AccountFormViewModel
        {
            IsEdit = true,
            Account = account,
            ParentAccounts = parentAccounts
        });
    }

    public async Task<IActionResult> OnPostAsync(AccountFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        FinanceAccount? account;

        if (input.Id.HasValue)
        {
            account = await _context.Accounts.FindAsync(input.Id.Value);
            if (account == null)
                return NotFound();
        }
        else
        {
            // Check if code already exists
            var codeExists = await _context.Accounts.AnyAsync(a => a.Code == input.Code);
            if (codeExists)
            {
                return BadRequest("An account with this code already exists.");
            }

            account = new FinanceAccount
            {
                Id = Guid.NewGuid()
            };
            _context.Accounts.Add(account);
        }

        account.Code = input.Code;
        account.Name = input.Name;
        account.Description = input.Description;
        account.AccountType = input.AccountType;
        account.AccountSubType = input.AccountSubType;
        account.ParentAccountId = input.ParentAccountId;
        account.OpeningBalance = input.OpeningBalance;
        account.CurrentBalance = input.CurrentBalance ?? input.OpeningBalance;
        account.Currency = input.Currency ?? "USD";
        account.IsActive = input.IsActive;
        account.AllowDirectPosting = input.AllowDirectPosting;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
            return NotFound();

        if (account.IsSystemAccount)
        {
            return BadRequest("Cannot delete system accounts.");
        }

        var hasTransactions = await _context.JournalEntryLines.AnyAsync(l => l.AccountId == id);
        if (hasTransactions)
        {
            return BadRequest("Cannot delete account with transactions. Deactivate instead.");
        }

        var hasChildren = await _context.Accounts.AnyAsync(a => a.ParentAccountId == id);
        if (hasChildren)
        {
            return BadRequest("Cannot delete account with child accounts.");
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }
}

public class AccountsTableViewModel
{
    public List<FinanceAccount> Accounts { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class AccountFormViewModel
{
    public bool IsEdit { get; set; }
    public FinanceAccount? Account { get; set; }
    public List<FinanceAccount> ParentAccounts { get; set; } = new();
}

public class AccountFormInput
{
    public Guid? Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccountType AccountType { get; set; }
    public AccountSubType? AccountSubType { get; set; }
    public Guid? ParentAccountId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? CurrentBalance { get; set; }
    public string? Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowDirectPosting { get; set; } = true;
}
