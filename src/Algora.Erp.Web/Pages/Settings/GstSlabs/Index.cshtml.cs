using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Settings.GstSlabs;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public List<GstSlab> GstSlabs { get; set; } = new();

    public async Task OnGetAsync()
    {
        GstSlabs = await _context.GstSlabs
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync()
    {
        var slabs = await _context.GstSlabs
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();

        return Partial("_GstSlabsTableRows", slabs);
    }

    public async Task<IActionResult> OnPostAsync(GstSlabInput input)
    {
        // Auto-calculate CGST/SGST if not provided
        if (input.CgstRate == 0 && input.SgstRate == 0 && input.Rate > 0)
        {
            input.CgstRate = input.Rate / 2;
            input.SgstRate = input.Rate / 2;
        }
        if (input.IgstRate == 0 && input.Rate > 0)
        {
            input.IgstRate = input.Rate;
        }

        if (input.Id == Guid.Empty)
        {
            var slab = new GstSlab
            {
                Name = input.Name,
                Rate = input.Rate,
                CgstRate = input.CgstRate,
                SgstRate = input.SgstRate,
                IgstRate = input.IgstRate,
                HsnCodes = input.HsnCodes,
                IsDefault = input.IsDefault,
                IsActive = input.IsActive,
                DisplayOrder = input.DisplayOrder
            };

            if (input.IsDefault)
            {
                await _context.GstSlabs
                    .Where(g => g.IsDefault)
                    .ExecuteUpdateAsync(g => g.SetProperty(x => x.IsDefault, false));
            }

            _context.GstSlabs.Add(slab);
        }
        else
        {
            var slab = await _context.GstSlabs.FindAsync(input.Id);
            if (slab == null) return NotFound();

            slab.Name = input.Name;
            slab.Rate = input.Rate;
            slab.CgstRate = input.CgstRate;
            slab.SgstRate = input.SgstRate;
            slab.IgstRate = input.IgstRate;
            slab.HsnCodes = input.HsnCodes;
            slab.IsActive = input.IsActive;
            slab.DisplayOrder = input.DisplayOrder;

            if (input.IsDefault && !slab.IsDefault)
            {
                await _context.GstSlabs
                    .Where(g => g.IsDefault)
                    .ExecuteUpdateAsync(g => g.SetProperty(x => x.IsDefault, false));
                slab.IsDefault = true;
            }
        }

        await _context.SaveChangesAsync();
        return await OnGetTableAsync();
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var slab = await _context.GstSlabs.FindAsync(id);
        if (slab == null) return NotFound();

        slab.IsDeleted = true;
        slab.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync();
    }
}

public class GstSlabInput
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }
    public string? HsnCodes { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
