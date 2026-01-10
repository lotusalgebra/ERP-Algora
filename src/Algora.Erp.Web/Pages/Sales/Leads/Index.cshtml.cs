using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Sales.Leads;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int QualifiedLeads { get; set; }
    public decimal PipelineValue { get; set; }

    public async Task OnGetAsync()
    {
        TotalLeads = await _context.Leads.CountAsync();
        NewLeads = await _context.Leads.CountAsync(l => l.Status == LeadStatus.New);
        QualifiedLeads = await _context.Leads.CountAsync(l => l.Status == LeadStatus.Qualified || l.Status == LeadStatus.Proposal || l.Status == LeadStatus.Negotiation);
        PipelineValue = await _context.Leads
            .Where(l => l.Status != LeadStatus.Won && l.Status != LeadStatus.Lost && l.Status != LeadStatus.Unqualified && l.EstimatedValue.HasValue)
            .SumAsync(l => l.EstimatedValue!.Value);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, string? sourceFilter, string? ratingFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Leads.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(l =>
                l.Name.ToLower().Contains(search) ||
                (l.Company != null && l.Company.ToLower().Contains(search)) ||
                (l.Email != null && l.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<LeadStatus>(statusFilter, out var status))
        {
            query = query.Where(l => l.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(sourceFilter) && Enum.TryParse<LeadSource>(sourceFilter, out var source))
        {
            query = query.Where(l => l.Source == source);
        }

        if (!string.IsNullOrWhiteSpace(ratingFilter) && Enum.TryParse<LeadRating>(ratingFilter, out var rating))
        {
            query = query.Where(l => l.Rating == rating);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var leads = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_LeadsTableRows", new LeadsTableViewModel
        {
            Leads = leads,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        return Partial("_LeadForm", new LeadFormViewModel { IsEdit = false });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return NotFound();

        return Partial("_LeadForm", new LeadFormViewModel { IsEdit = true, Lead = lead });
    }

    public async Task<IActionResult> OnPostAsync(LeadFormInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Lead? lead;

        if (input.Id.HasValue)
        {
            lead = await _context.Leads.FindAsync(input.Id.Value);
            if (lead == null) return NotFound();
        }
        else
        {
            lead = new Lead { Id = Guid.NewGuid() };
            _context.Leads.Add(lead);
        }

        lead.Name = input.Name;
        lead.Company = input.Company;
        lead.Email = input.Email;
        lead.Phone = input.Phone;
        lead.Website = input.Website;
        lead.Source = input.Source;
        lead.Status = input.Status;
        lead.Rating = input.Rating;
        lead.EstimatedValue = input.EstimatedValue;
        lead.EstimatedCloseInDays = input.EstimatedCloseInDays;
        lead.Address = input.Address;
        lead.City = input.City;
        lead.State = input.State;
        lead.Country = input.Country;
        lead.PostalCode = input.PostalCode;
        lead.NextFollowUpDate = input.NextFollowUpDate;
        lead.Notes = input.Notes;
        lead.Tags = input.Tags;

        await _context.SaveChangesAsync();
        return await OnGetTableAsync(null, null, null, null);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, LeadStatus status)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return NotFound();

        lead.Status = status;
        lead.LastContactDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null) return NotFound();

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null, null);
    }
}

public class LeadsTableViewModel
{
    public List<Lead> Leads { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Sales/Leads",
        Handler = "Table",
        HxTarget = "#leadsTableBody",
        HxInclude = "#searchInput,#statusFilter,#sourceFilter,#ratingFilter"
    };
}

public class LeadFormViewModel
{
    public bool IsEdit { get; set; }
    public Lead? Lead { get; set; }
}

public class LeadFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public LeadSource Source { get; set; }
    public LeadStatus Status { get; set; }
    public LeadRating Rating { get; set; }
    public decimal? EstimatedValue { get; set; }
    public int? EstimatedCloseInDays { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}
