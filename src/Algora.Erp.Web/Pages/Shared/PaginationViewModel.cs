namespace Algora.Erp.Web.Pages.Shared;

/// <summary>
/// Generic view model for pagination controls.
/// Use this with _Pagination.cshtml partial view for consistent pagination across all grids.
/// </summary>
public class PaginationViewModel
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Total number of records in the dataset
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalRecords / (double)PageSize) : 0;

    /// <summary>
    /// The URL path for pagination requests (e.g., "/Finance/Invoices")
    /// </summary>
    public string PageUrl { get; set; } = string.Empty;

    /// <summary>
    /// The handler name for HTMX requests (e.g., "Table")
    /// </summary>
    public string Handler { get; set; } = "Table";

    /// <summary>
    /// The target element ID for HTMX to swap content into (e.g., "#tableContent")
    /// </summary>
    public string HxTarget { get; set; } = "#tableContent";

    /// <summary>
    /// Comma-separated list of filter input IDs to include in pagination requests
    /// (e.g., "#searchInput,#statusFilter,#dateFrom")
    /// </summary>
    public string HxInclude { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of page buttons to show (default: 5)
    /// </summary>
    public int MaxVisiblePages { get; set; } = 5;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// First record number shown on current page (1-based)
    /// </summary>
    public int FirstRecord => TotalRecords == 0 ? 0 : (Page - 1) * PageSize + 1;

    /// <summary>
    /// Last record number shown on current page
    /// </summary>
    public int LastRecord => Math.Min(Page * PageSize, TotalRecords);

    /// <summary>
    /// Get the range of page numbers to display
    /// </summary>
    public (int Start, int End) GetPageRange()
    {
        if (TotalPages <= MaxVisiblePages)
        {
            return (1, TotalPages);
        }

        int halfVisible = MaxVisiblePages / 2;
        int start = Math.Max(1, Page - halfVisible);
        int end = Math.Min(TotalPages, start + MaxVisiblePages - 1);

        // Adjust start if we're near the end
        if (end == TotalPages)
        {
            start = Math.Max(1, TotalPages - MaxVisiblePages + 1);
        }

        return (start, end);
    }

    /// <summary>
    /// Build the HTMX URL for a specific page
    /// </summary>
    public string GetPageUrl(int pageNumber)
    {
        var url = $"{PageUrl}?handler={Handler}&page={pageNumber}&pageSize={PageSize}";
        return url;
    }
}

/// <summary>
/// Interface for view models that include pagination
/// </summary>
public interface IPaginatedViewModel
{
    PaginationViewModel Pagination { get; }
}
