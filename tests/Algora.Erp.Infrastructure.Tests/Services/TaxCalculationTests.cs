using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Algora.Erp.Infrastructure.Tests.Services;

public class TaxCalculationTests
{
    private readonly TaxConfigurationService _service;
    private readonly Mock<IApplicationDbContext> _mockContext;

    public TaxCalculationTests()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _service = new TaxConfigurationService(_mockContext.Object);
    }

    [Fact]
    public void CalculateTax_WithSimpleTaxRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var taxSlab = new TaxSlab
        {
            Name = "VAT 20%",
            Rate = 20,
            CentralRate = 0,
            RegionalRate = 0,
            InterRegionalRate = 0
        };
        var amount = 1000m;

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: false);

        // Assert
        result.TaxableAmount.Should().Be(1000m);
        result.TaxRate.Should().Be(20m);
        result.TotalTaxAmount.Should().Be(200m);
        result.TotalAmount.Should().Be(1200m);
    }

    [Fact]
    public void CalculateTax_WithSplitTax_ShouldCalculateCentralAndRegional()
    {
        // Arrange - Indian GST style (CGST + SGST)
        var taxSlab = new TaxSlab
        {
            Name = "GST 18%",
            Rate = 18,
            CentralRate = 9, // CGST
            RegionalRate = 9, // SGST
            InterRegionalRate = 18
        };
        var amount = 10000m;

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: false);

        // Assert
        result.TaxableAmount.Should().Be(10000m);
        result.CentralTaxRate.Should().Be(9m);
        result.RegionalTaxRate.Should().Be(9m);
        result.CentralTaxAmount.Should().Be(900m);
        result.RegionalTaxAmount.Should().Be(900m);
        result.TotalTaxAmount.Should().Be(1800m);
        result.TotalAmount.Should().Be(11800m);
    }

    [Fact]
    public void CalculateTax_WithInterRegionalTax_ShouldCalculateIgst()
    {
        // Arrange - Indian IGST style (inter-state)
        var taxSlab = new TaxSlab
        {
            Name = "GST 18%",
            Rate = 18,
            CentralRate = 9,
            RegionalRate = 9,
            InterRegionalRate = 18
        };
        var amount = 10000m;

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: true);

        // Assert
        result.TaxableAmount.Should().Be(10000m);
        result.IsInterRegional.Should().BeTrue();
        result.InterRegionalTaxRate.Should().Be(18m);
        result.InterRegionalTaxAmount.Should().Be(1800m);
        result.TotalTaxAmount.Should().Be(1800m);
        result.TotalAmount.Should().Be(11800m);
        // Central and Regional should be 0 for inter-regional
        result.CentralTaxAmount.Should().Be(0m);
        result.RegionalTaxAmount.Should().Be(0m);
    }

    [Fact]
    public void CalculateTax_WithZeroRate_ShouldReturnZeroTax()
    {
        // Arrange
        var taxSlab = new TaxSlab
        {
            Name = "Zero Rate",
            Rate = 0,
            CentralRate = 0,
            RegionalRate = 0,
            InterRegionalRate = 0,
            IsZeroRated = true
        };
        var amount = 5000m;

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: false);

        // Assert
        result.TaxableAmount.Should().Be(5000m);
        result.TotalTaxAmount.Should().Be(0m);
        result.TotalAmount.Should().Be(5000m);
    }

    [Fact]
    public void CalculateTax_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange
        var taxSlab = new TaxSlab
        {
            Name = "GST 5%",
            Rate = 5,
            CentralRate = 2.5m,
            RegionalRate = 2.5m,
            InterRegionalRate = 5
        };
        var amount = 333.33m; // Amount that would create many decimal places

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: false);

        // Assert
        result.CentralTaxAmount.Should().Be(8.33m); // 333.33 * 0.025 = 8.33325 -> 8.33
        result.RegionalTaxAmount.Should().Be(8.33m);
        result.TotalTaxAmount.Should().Be(16.66m);
    }

    [Theory]
    [InlineData(100, 5, 5)]
    [InlineData(100, 12, 12)]
    [InlineData(100, 18, 18)]
    [InlineData(100, 28, 28)]
    [InlineData(1000, 18, 180)]
    [InlineData(5000, 12, 600)]
    public void CalculateTax_WithVariousRates_ShouldCalculateCorrectly(
        decimal amount, decimal rate, decimal expectedTax)
    {
        // Arrange
        var taxSlab = new TaxSlab
        {
            Name = $"Tax {rate}%",
            Rate = rate,
            CentralRate = rate / 2,
            RegionalRate = rate / 2,
            InterRegionalRate = rate
        };

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: true);

        // Assert
        result.TotalTaxAmount.Should().Be(expectedTax);
        result.TotalAmount.Should().Be(amount + expectedTax);
    }

    [Fact]
    public void CalculateTax_WithDecimalAmount_ShouldHandlePrecision()
    {
        // Arrange
        var taxSlab = new TaxSlab
        {
            Name = "VAT 10%",
            Rate = 10,
            CentralRate = 0,
            RegionalRate = 0,
            InterRegionalRate = 0
        };
        var amount = 99.99m;

        // Act
        var result = _service.CalculateTax(amount, taxSlab, isInterRegional: false);

        // Assert
        result.TaxableAmount.Should().Be(99.99m);
        result.TotalTaxAmount.Should().Be(10m); // 99.99 * 0.10 = 9.999 -> rounded to 10
        result.TotalAmount.Should().Be(109.99m);
    }
}
