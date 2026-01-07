using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Settings.Locations;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public List<OfficeLocation> Locations { get; set; } = new();
    public List<SelectListItem> States { get; set; } = new();
    public List<SelectListItem> Currencies { get; set; } = new();

    public async Task OnGetAsync()
    {
        Locations = await _context.OfficeLocations
            .Include(l => l.State)
            .Include(l => l.DefaultCurrency)
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();

        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnGetTableAsync()
    {
        var locations = await _context.OfficeLocations
            .Include(l => l.State)
            .Include(l => l.DefaultCurrency)
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();

        return Partial("_LocationsTableRows", locations);
    }

    public async Task<IActionResult> OnPostAsync(LocationInput input)
    {
        if (input.Id == Guid.Empty)
        {
            var location = new OfficeLocation
            {
                Code = input.Code.ToUpper(),
                Name = input.Name,
                Type = input.Type,
                AddressLine1 = input.AddressLine1,
                AddressLine2 = input.AddressLine2,
                City = input.City,
                StateId = input.StateId,
                PostalCode = input.PostalCode,
                Country = input.Country,
                GstNumber = input.GstNumber?.ToUpper(),
                IsGstRegistered = input.IsGstRegistered,
                GstRegistrationType = input.GstRegistrationType,
                DefaultCurrencyId = input.DefaultCurrencyId,
                Phone = input.Phone,
                Email = input.Email,
                ContactPerson = input.ContactPerson,
                IsHeadOffice = input.IsHeadOffice,
                IsActive = input.IsActive,
                DisplayOrder = input.DisplayOrder
            };

            if (input.IsHeadOffice)
            {
                await _context.OfficeLocations
                    .Where(l => l.IsHeadOffice)
                    .ExecuteUpdateAsync(l => l.SetProperty(x => x.IsHeadOffice, false));
            }

            _context.OfficeLocations.Add(location);
        }
        else
        {
            var location = await _context.OfficeLocations.FindAsync(input.Id);
            if (location == null) return NotFound();

            location.Code = input.Code.ToUpper();
            location.Name = input.Name;
            location.Type = input.Type;
            location.AddressLine1 = input.AddressLine1;
            location.AddressLine2 = input.AddressLine2;
            location.City = input.City;
            location.StateId = input.StateId;
            location.PostalCode = input.PostalCode;
            location.Country = input.Country;
            location.GstNumber = input.GstNumber?.ToUpper();
            location.IsGstRegistered = input.IsGstRegistered;
            location.GstRegistrationType = input.GstRegistrationType;
            location.DefaultCurrencyId = input.DefaultCurrencyId;
            location.Phone = input.Phone;
            location.Email = input.Email;
            location.ContactPerson = input.ContactPerson;
            location.IsActive = input.IsActive;
            location.DisplayOrder = input.DisplayOrder;

            if (input.IsHeadOffice && !location.IsHeadOffice)
            {
                await _context.OfficeLocations
                    .Where(l => l.IsHeadOffice)
                    .ExecuteUpdateAsync(l => l.SetProperty(x => x.IsHeadOffice, false));
                location.IsHeadOffice = true;
            }
        }

        await _context.SaveChangesAsync();
        return await OnGetTableAsync();
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var location = await _context.OfficeLocations.FindAsync(id);
        if (location == null) return NotFound();

        location.IsDeleted = true;
        location.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync();
    }

    private async Task LoadDropdownsAsync()
    {
        var states = await _context.IndianStates
            .Where(s => !s.IsDeleted && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        States = states.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.Name} ({s.Code})"
        }).ToList();

        var currencies = await _context.Currencies
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        Currencies = currencies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = $"{c.Code} - {c.Name}"
        }).ToList();
    }
}

public class LocationInput
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public OfficeType Type { get; set; } = OfficeType.Branch;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public Guid? StateId { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "India";
    public string? GstNumber { get; set; }
    public bool IsGstRegistered { get; set; } = true;
    public GstRegistrationType GstRegistrationType { get; set; } = GstRegistrationType.Regular;
    public Guid? DefaultCurrencyId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
