using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Reports.Cancellations;

public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    // Stats
    public int TotalCancellations { get; set; }
    public int ThisMonth { get; set; }
    public int ThisWeek { get; set; }
    public Dictionary<string, int> ByDocumentType { get; set; } = new();

    public async Task OnGetAsync()
    {
        var cancellations = await _context.CancellationLogs
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        TotalCancellations = cancellations.Count;
        ThisMonth = cancellations.Count(c => c.CancelledAt >= DateTime.UtcNow.AddDays(-30));
        ThisWeek = cancellations.Count(c => c.CancelledAt >= DateTime.UtcNow.AddDays(-7));
        ByDocumentType = cancellations
            .GroupBy(c => c.DocumentType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IActionResult> OnGetTableAsync(
        string? search,
        string? documentTypeFilter,
        int? reasonCategoryFilter,
        DateTime? fromDate,
        DateTime? toDate,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.CancellationLogs
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.DocumentNumber.ToLower().Contains(search) ||
                (c.CancellationReason != null && c.CancellationReason.ToLower().Contains(search)) ||
                (c.CancelledByName != null && c.CancelledByName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(documentTypeFilter))
        {
            query = query.Where(c => c.DocumentType == documentTypeFilter);
        }

        if (reasonCategoryFilter.HasValue)
        {
            query = query.Where(c => (int)c.ReasonCategory == reasonCategoryFilter.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CancelledAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.CancelledAt <= toDate.Value.AddDays(1));
        }

        var totalRecords = await query.CountAsync();
        var cancellations = await query
            .OrderByDescending(c => c.CancelledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new CancellationsTableViewModel
        {
            Cancellations = cancellations,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };

        return Partial("_CancellationsTableRows", viewModel);
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var cancellation = await _context.CancellationLogs
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cancellation == null)
            return NotFound();

        return Partial("_CancellationDetails", cancellation);
    }

    public async Task<IActionResult> OnGetExportAsync(
        string? documentTypeFilter,
        int? reasonCategoryFilter,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = _context.CancellationLogs
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(documentTypeFilter))
        {
            query = query.Where(c => c.DocumentType == documentTypeFilter);
        }

        if (reasonCategoryFilter.HasValue)
        {
            query = query.Where(c => (int)c.ReasonCategory == reasonCategoryFilter.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CancelledAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.CancelledAt <= toDate.Value.AddDays(1));
        }

        var cancellations = await query
            .OrderByDescending(c => c.CancelledAt)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Document Type,Document Number,Cancelled At,Cancelled By,Reason Category,Reason,Notes");

        foreach (var c in cancellations)
        {
            var reason = EscapeCsvField(c.CancellationReason ?? "");
            var notes = EscapeCsvField(c.Notes ?? "");
            csv.AppendLine($"{c.DocumentType},{c.DocumentNumber},{c.CancelledAt:yyyy-MM-dd HH:mm},{c.CancelledByName ?? ""},{c.ReasonCategory},{reason},{notes}");
        }

        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"cancellations_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}

public class CancellationsTableViewModel
{
    public List<CancellationLog> Cancellations { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
