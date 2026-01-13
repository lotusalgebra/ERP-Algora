using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Settings.Currencies;

[Authorize(Policy = "CanViewSettings")]
[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public List<Currency> Currencies { get; set; } = new();
    public Currency? BaseCurrency { get; set; }

    public async Task OnGetAsync()
    {
        Currencies = await _context.Currencies
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        BaseCurrency = Currencies.FirstOrDefault(c => c.IsBaseCurrency);
    }

    public async Task<IActionResult> OnGetTableAsync()
    {
        var currencies = await _context.Currencies
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return Partial("_CurrenciesTableRows", currencies);
    }

    public async Task<IActionResult> OnPostAsync(CurrencyInput input)
    {
        if (input.Id == Guid.Empty)
        {
            // Create new
            var currency = new Currency
            {
                Code = input.Code.ToUpper(),
                Name = input.Name,
                Symbol = input.Symbol,
                SymbolPosition = input.SymbolPosition,
                DecimalPlaces = input.DecimalPlaces,
                DecimalSeparator = input.DecimalSeparator,
                ThousandsSeparator = input.ThousandsSeparator,
                ExchangeRate = input.ExchangeRate,
                IsBaseCurrency = input.IsBaseCurrency,
                IsActive = input.IsActive,
                DisplayOrder = input.DisplayOrder
            };

            // If setting as base currency, unset others
            if (input.IsBaseCurrency)
            {
                await _context.Currencies
                    .Where(c => c.IsBaseCurrency)
                    .ExecuteUpdateAsync(c => c.SetProperty(x => x.IsBaseCurrency, false));
            }

            _context.Currencies.Add(currency);
        }
        else
        {
            // Update existing
            var currency = await _context.Currencies.FindAsync(input.Id);
            if (currency == null) return NotFound();

            currency.Code = input.Code.ToUpper();
            currency.Name = input.Name;
            currency.Symbol = input.Symbol;
            currency.SymbolPosition = input.SymbolPosition;
            currency.DecimalPlaces = input.DecimalPlaces;
            currency.DecimalSeparator = input.DecimalSeparator;
            currency.ThousandsSeparator = input.ThousandsSeparator;
            currency.ExchangeRate = input.ExchangeRate;
            currency.IsActive = input.IsActive;
            currency.DisplayOrder = input.DisplayOrder;

            // Handle base currency change
            if (input.IsBaseCurrency && !currency.IsBaseCurrency)
            {
                await _context.Currencies
                    .Where(c => c.IsBaseCurrency)
                    .ExecuteUpdateAsync(c => c.SetProperty(x => x.IsBaseCurrency, false));
                currency.IsBaseCurrency = true;
            }
        }

        await _context.SaveChangesAsync();
        return await OnGetTableAsync();
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var currency = await _context.Currencies.FindAsync(id);
        if (currency == null) return NotFound();

        if (currency.IsBaseCurrency)
        {
            return BadRequest("Cannot delete the base currency.");
        }

        currency.IsDeleted = true;
        currency.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync();
    }

    public async Task<IActionResult> OnPostSetBaseAsync(Guid id)
    {
        await _context.Currencies
            .Where(c => c.IsBaseCurrency)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.IsBaseCurrency, false));

        await _context.Currencies
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.IsBaseCurrency, true));

        return await OnGetTableAsync();
    }
}

public class CurrencyInput
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string SymbolPosition { get; set; } = "before";
    public int DecimalPlaces { get; set; } = 2;
    public string DecimalSeparator { get; set; } = ".";
    public string ThousandsSeparator { get; set; } = ",";
    public decimal ExchangeRate { get; set; } = 1.0m;
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
