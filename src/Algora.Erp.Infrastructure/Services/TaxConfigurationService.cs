using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services;

/// <summary>
/// Service for managing tenant-specific tax configurations
/// </summary>
public class TaxConfigurationService : ITaxConfigurationService
{
    private readonly IApplicationDbContext _context;

    public TaxConfigurationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaxConfiguration?> GetCurrentTaxConfigurationAsync()
    {
        return await _context.TaxConfigurations
            .Include(tc => tc.TaxSlabs.Where(s => s.IsActive && !s.IsDeleted))
            .Include(tc => tc.TaxRegions.Where(r => r.IsActive && !r.IsDeleted))
            .Where(tc => tc.IsDefault && tc.IsActive && !tc.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<TaxConfiguration?> GetByIdAsync(Guid id)
    {
        return await _context.TaxConfigurations
            .Include(tc => tc.TaxSlabs.Where(s => !s.IsDeleted))
            .Include(tc => tc.TaxRegions.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(tc => tc.Id == id && !tc.IsDeleted);
    }

    public async Task<List<TaxConfiguration>> GetAllAsync()
    {
        return await _context.TaxConfigurations
            .Where(tc => !tc.IsDeleted)
            .OrderByDescending(tc => tc.IsDefault)
            .ThenBy(tc => tc.Name)
            .ToListAsync();
    }

    public async Task<List<TaxSlab>> GetActiveTaxSlabsAsync()
    {
        var config = await GetCurrentTaxConfigurationAsync();
        if (config == null)
            return new List<TaxSlab>();

        return config.TaxSlabs
            .Where(s => s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Rate)
            .ToList();
    }

    public async Task<List<TaxRegion>> GetActiveTaxRegionsAsync()
    {
        var config = await GetCurrentTaxConfigurationAsync();
        if (config == null)
            return new List<TaxRegion>();

        return config.TaxRegions
            .Where(r => r.IsActive && !r.IsDeleted)
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.Name)
            .ToList();
    }

    public async Task<TaxSlab?> GetTaxSlabByIdAsync(Guid id)
    {
        return await _context.TaxSlabs
            .Include(s => s.TaxConfiguration)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public TaxCalculationResult CalculateTax(decimal amount, TaxSlab taxSlab, bool isInterRegional = false)
    {
        var result = new TaxCalculationResult
        {
            TaxableAmount = amount,
            TaxRate = taxSlab.Rate,
            IsInterRegional = isInterRegional
        };

        if (isInterRegional && taxSlab.InterRegionalRate > 0)
        {
            // Inter-regional tax (e.g., IGST in India)
            result.InterRegionalTaxRate = taxSlab.InterRegionalRate;
            result.InterRegionalTaxAmount = Math.Round(amount * (taxSlab.InterRegionalRate / 100), 2);
            result.TotalTaxAmount = result.InterRegionalTaxAmount;
        }
        else if (taxSlab.CentralRate > 0 || taxSlab.RegionalRate > 0)
        {
            // Split tax (e.g., CGST + SGST in India)
            result.CentralTaxRate = taxSlab.CentralRate;
            result.RegionalTaxRate = taxSlab.RegionalRate;
            result.CentralTaxAmount = Math.Round(amount * (taxSlab.CentralRate / 100), 2);
            result.RegionalTaxAmount = Math.Round(amount * (taxSlab.RegionalRate / 100), 2);
            result.TotalTaxAmount = result.CentralTaxAmount + result.RegionalTaxAmount;
        }
        else
        {
            // Simple single-rate tax (e.g., VAT in UK)
            result.TotalTaxAmount = Math.Round(amount * (taxSlab.Rate / 100), 2);
        }

        result.TotalAmount = amount + result.TotalTaxAmount;

        return result;
    }

    public async Task<TaxConfiguration> CreateFromTemplateAsync(string countryCode)
    {
        var template = countryCode.ToUpperInvariant() switch
        {
            "IN" => TaxConfigurationTemplates.India,
            "GB" => TaxConfigurationTemplates.UnitedKingdom,
            "US" => TaxConfigurationTemplates.USA,
            "CA" => TaxConfigurationTemplates.Canada,
            "AU" => TaxConfigurationTemplates.Australia,
            "EU" => TaxConfigurationTemplates.EU,
            "AE" => TaxConfigurationTemplates.UAE,
            "SG" => TaxConfigurationTemplates.Singapore,
            _ => TaxConfigurationTemplates.NoTax
        };

        // Check if any configuration exists
        var existingDefault = await _context.TaxConfigurations
            .AnyAsync(tc => tc.IsDefault && !tc.IsDeleted);

        template.IsDefault = !existingDefault;

        _context.TaxConfigurations.Add(template);
        await _context.SaveChangesAsync();

        // Add default tax slabs for the template
        await AddDefaultTaxSlabsAsync(template);

        // Add default regions for India
        if (countryCode.ToUpperInvariant() == "IN")
        {
            await AddIndianStatesAsRegionsAsync(template);
        }

        return template;
    }

    private async Task AddDefaultTaxSlabsAsync(TaxConfiguration config)
    {
        var slabs = config.TaxSystem switch
        {
            TaxSystem.GST when config.CountryCode == "IN" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "GST 0%", Code = "GST0", Rate = 0, CentralRate = 0, RegionalRate = 0, InterRegionalRate = 0, IsZeroRated = true, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "GST 5%", Code = "GST5", Rate = 5, CentralRate = 2.5m, RegionalRate = 2.5m, InterRegionalRate = 5, DisplayOrder = 2 },
                new() { TaxConfigurationId = config.Id, Name = "GST 12%", Code = "GST12", Rate = 12, CentralRate = 6, RegionalRate = 6, InterRegionalRate = 12, DisplayOrder = 3 },
                new() { TaxConfigurationId = config.Id, Name = "GST 18%", Code = "GST18", Rate = 18, CentralRate = 9, RegionalRate = 9, InterRegionalRate = 18, IsDefault = true, DisplayOrder = 4 },
                new() { TaxConfigurationId = config.Id, Name = "GST 28%", Code = "GST28", Rate = 28, CentralRate = 14, RegionalRate = 14, InterRegionalRate = 28, DisplayOrder = 5 },
                new() { TaxConfigurationId = config.Id, Name = "Exempt", Code = "EXEMPT", Rate = 0, IsExempt = true, DisplayOrder = 6 }
            },
            TaxSystem.VAT when config.CountryCode == "GB" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "Standard Rate (20%)", Code = "STD", Rate = 20, IsDefault = true, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "Reduced Rate (5%)", Code = "RED", Rate = 5, DisplayOrder = 2 },
                new() { TaxConfigurationId = config.Id, Name = "Zero Rate (0%)", Code = "ZERO", Rate = 0, IsZeroRated = true, DisplayOrder = 3 },
                new() { TaxConfigurationId = config.Id, Name = "Exempt", Code = "EXEMPT", Rate = 0, IsExempt = true, DisplayOrder = 4 }
            },
            TaxSystem.SalesTax when config.CountryCode == "US" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "No Tax", Code = "NOTAX", Rate = 0, IsZeroRated = true, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "Standard Sales Tax", Code = "STD", Rate = 0, RegionalRate = 0, IsDefault = true, DisplayOrder = 2, Description = "Rate varies by state" }
            },
            TaxSystem.GST when config.CountryCode == "AU" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "GST (10%)", Code = "GST", Rate = 10, IsDefault = true, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "GST Free", Code = "FREE", Rate = 0, IsZeroRated = true, DisplayOrder = 2 },
                new() { TaxConfigurationId = config.Id, Name = "Input Taxed", Code = "INPUT", Rate = 0, IsExempt = true, DisplayOrder = 3 }
            },
            TaxSystem.VAT when config.CountryCode == "AE" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "VAT (5%)", Code = "VAT5", Rate = 5, IsDefault = true, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "Zero Rated", Code = "ZERO", Rate = 0, IsZeroRated = true, DisplayOrder = 2 },
                new() { TaxConfigurationId = config.Id, Name = "Exempt", Code = "EXEMPT", Rate = 0, IsExempt = true, DisplayOrder = 3 }
            },
            TaxSystem.GST when config.CountryCode == "SG" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "GST (9%)", Code = "GST9", Rate = 9, IsDefault = true, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "Zero Rated", Code = "ZERO", Rate = 0, IsZeroRated = true, DisplayOrder = 2 },
                new() { TaxConfigurationId = config.Id, Name = "Exempt", Code = "EXEMPT", Rate = 0, IsExempt = true, DisplayOrder = 3 }
            },
            TaxSystem.GST_PST when config.CountryCode == "CA" => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "GST (5%)", Code = "GST", Rate = 5, CentralRate = 5, DisplayOrder = 1 },
                new() { TaxConfigurationId = config.Id, Name = "HST (13%)", Code = "HST13", Rate = 13, InterRegionalRate = 13, IsDefault = true, DisplayOrder = 2 },
                new() { TaxConfigurationId = config.Id, Name = "HST (15%)", Code = "HST15", Rate = 15, InterRegionalRate = 15, DisplayOrder = 3 },
                new() { TaxConfigurationId = config.Id, Name = "Zero Rated", Code = "ZERO", Rate = 0, IsZeroRated = true, DisplayOrder = 4 },
                new() { TaxConfigurationId = config.Id, Name = "Exempt", Code = "EXEMPT", Rate = 0, IsExempt = true, DisplayOrder = 5 }
            },
            _ => new List<TaxSlab>
            {
                new() { TaxConfigurationId = config.Id, Name = "No Tax", Code = "NOTAX", Rate = 0, IsDefault = true, DisplayOrder = 1 }
            }
        };

        _context.TaxSlabs.AddRange(slabs);
        await _context.SaveChangesAsync();
    }

    private async Task AddIndianStatesAsRegionsAsync(TaxConfiguration config)
    {
        // Get existing Indian states from the IndianStates table
        var indianStates = await _context.IndianStates
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var displayOrder = 1;
        foreach (var state in indianStates)
        {
            var region = new TaxRegion
            {
                TaxConfigurationId = config.Id,
                Code = state.Code,
                Name = state.Name,
                ShortName = state.ShortName,
                IsUnionTerritory = state.IsUnionTerritory,
                DisplayOrder = displayOrder++
            };
            _context.TaxRegions.Add(region);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<TaxConfiguration> SaveAsync(TaxConfiguration configuration)
    {
        if (configuration.Id == Guid.Empty)
        {
            _context.TaxConfigurations.Add(configuration);
        }
        else
        {
            var existing = await _context.TaxConfigurations.FindAsync(configuration.Id);
            if (existing != null)
            {
                existing.Name = configuration.Name;
                existing.CountryCode = configuration.CountryCode;
                existing.CountryName = configuration.CountryName;
                existing.TaxSystem = configuration.TaxSystem;
                existing.TaxSystemName = configuration.TaxSystemName;
                existing.TaxIdLabel = configuration.TaxIdLabel;
                existing.TaxIdFormat = configuration.TaxIdFormat;
                existing.TaxIdExample = configuration.TaxIdExample;
                existing.HasRegionalTax = configuration.HasRegionalTax;
                existing.RegionLabel = configuration.RegionLabel;
                existing.HasInterRegionalTax = configuration.HasInterRegionalTax;
                existing.CentralTaxLabel = configuration.CentralTaxLabel;
                existing.RegionalTaxLabel = configuration.RegionalTaxLabel;
                existing.InterRegionalTaxLabel = configuration.InterRegionalTaxLabel;
                existing.CombinedTaxLabel = configuration.CombinedTaxLabel;
                existing.ProductCodeLabel = configuration.ProductCodeLabel;
                existing.ServiceCodeLabel = configuration.ServiceCodeLabel;
                existing.CalculationMethod = configuration.CalculationMethod;
                existing.DecimalPlaces = configuration.DecimalPlaces;
                existing.RoundAtLineLevel = configuration.RoundAtLineLevel;
                existing.DefaultCurrencyCode = configuration.DefaultCurrencyCode;
                existing.DefaultCurrencySymbol = configuration.DefaultCurrencySymbol;
                existing.IsActive = configuration.IsActive;
            }
        }

        await _context.SaveChangesAsync();
        return configuration;
    }

    public async Task<TaxSlab> SaveTaxSlabAsync(TaxSlab slab)
    {
        if (slab.Id == Guid.Empty)
        {
            _context.TaxSlabs.Add(slab);
        }
        else
        {
            var existing = await _context.TaxSlabs.FindAsync(slab.Id);
            if (existing != null)
            {
                existing.Name = slab.Name;
                existing.Code = slab.Code;
                existing.Description = slab.Description;
                existing.Rate = slab.Rate;
                existing.CentralRate = slab.CentralRate;
                existing.RegionalRate = slab.RegionalRate;
                existing.InterRegionalRate = slab.InterRegionalRate;
                existing.ApplicableCodes = slab.ApplicableCodes;
                existing.IsDefault = slab.IsDefault;
                existing.IsZeroRated = slab.IsZeroRated;
                existing.IsExempt = slab.IsExempt;
                existing.IsActive = slab.IsActive;
                existing.DisplayOrder = slab.DisplayOrder;
            }
        }

        await _context.SaveChangesAsync();
        return slab;
    }

    public async Task<TaxRegion> SaveTaxRegionAsync(TaxRegion region)
    {
        if (region.Id == Guid.Empty)
        {
            _context.TaxRegions.Add(region);
        }
        else
        {
            var existing = await _context.TaxRegions.FindAsync(region.Id);
            if (existing != null)
            {
                existing.Code = region.Code;
                existing.Name = region.Name;
                existing.ShortName = region.ShortName;
                existing.RegionalTaxRate = region.RegionalTaxRate;
                existing.IsUnionTerritory = region.IsUnionTerritory;
                existing.HasLocalTax = region.HasLocalTax;
                existing.LocalTaxRate = region.LocalTaxRate;
                existing.IsActive = region.IsActive;
                existing.DisplayOrder = region.DisplayOrder;
            }
        }

        await _context.SaveChangesAsync();
        return region;
    }

    public async Task DeleteAsync(Guid id)
    {
        var config = await _context.TaxConfigurations.FindAsync(id);
        if (config != null)
        {
            config.IsDeleted = true;
            config.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetAsDefaultAsync(Guid id)
    {
        // Remove default from all existing configurations
        var allConfigs = await _context.TaxConfigurations
            .Where(tc => !tc.IsDeleted)
            .ToListAsync();

        foreach (var config in allConfigs)
        {
            config.IsDefault = config.Id == id;
        }

        await _context.SaveChangesAsync();
    }
}
