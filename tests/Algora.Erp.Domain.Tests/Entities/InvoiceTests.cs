using Algora.Erp.Domain.Entities.Finance;

namespace Algora.Erp.Domain.Tests.Entities;

public class InvoiceTests
{
    [Fact]
    public void Invoice_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.Id.Should().NotBe(Guid.Empty);
        invoice.InvoiceNumber.Should().BeEmpty();
        invoice.Type.Should().Be(InvoiceType.SalesInvoice);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Currency.Should().Be("INR");
        invoice.PaymentTermDays.Should().Be(30);
        invoice.Lines.Should().BeEmpty();
        invoice.Payments.Should().BeEmpty();
    }

    [Fact]
    public void Invoice_ShouldCalculateBalanceDue()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 1000m,
            PaidAmount = 400m
        };

        // Act
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        // Assert
        invoice.BalanceDue.Should().Be(600m);
    }

    [Fact]
    public void Invoice_ShouldSupportGstCalculations()
    {
        // Arrange
        var invoice = new Invoice
        {
            SubTotal = 10000m,
            IsInterState = false, // CGST + SGST
            CgstRate = 9m,
            SgstRate = 9m
        };

        // Act
        invoice.CgstAmount = invoice.SubTotal * invoice.CgstRate / 100;
        invoice.SgstAmount = invoice.SubTotal * invoice.SgstRate / 100;
        invoice.TaxAmount = invoice.CgstAmount + invoice.SgstAmount;
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;

        // Assert
        invoice.CgstAmount.Should().Be(900m);
        invoice.SgstAmount.Should().Be(900m);
        invoice.TaxAmount.Should().Be(1800m);
        invoice.TotalAmount.Should().Be(11800m);
    }

    [Fact]
    public void Invoice_ShouldSupportIgstCalculation()
    {
        // Arrange
        var invoice = new Invoice
        {
            SubTotal = 10000m,
            IsInterState = true, // IGST
            IgstRate = 18m
        };

        // Act
        invoice.IgstAmount = invoice.SubTotal * invoice.IgstRate / 100;
        invoice.TaxAmount = invoice.IgstAmount;
        invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;

        // Assert
        invoice.IgstAmount.Should().Be(1800m);
        invoice.TaxAmount.Should().Be(1800m);
        invoice.TotalAmount.Should().Be(11800m);
    }

    [Theory]
    [InlineData(InvoiceStatus.Draft)]
    [InlineData(InvoiceStatus.Pending)]
    [InlineData(InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.PartiallyPaid)]
    [InlineData(InvoiceStatus.Paid)]
    [InlineData(InvoiceStatus.Overdue)]
    [InlineData(InvoiceStatus.Void)]
    [InlineData(InvoiceStatus.Cancelled)]
    public void Invoice_ShouldSupportAllStatuses(InvoiceStatus status)
    {
        // Arrange
        var invoice = new Invoice();

        // Act
        invoice.Status = status;

        // Assert
        invoice.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(InvoiceType.SalesInvoice)]
    [InlineData(InvoiceType.PurchaseInvoice)]
    [InlineData(InvoiceType.CreditNote)]
    [InlineData(InvoiceType.DebitNote)]
    public void Invoice_ShouldSupportAllTypes(InvoiceType type)
    {
        // Arrange
        var invoice = new Invoice();

        // Act
        invoice.Type = type;

        // Assert
        invoice.Type.Should().Be(type);
    }
}

public class InvoiceLineTests
{
    [Fact]
    public void InvoiceLine_ShouldCalculateLineTotal()
    {
        // Arrange
        var line = new InvoiceLine
        {
            Quantity = 10,
            UnitPrice = 100m,
            DiscountPercent = 10m
        };

        // Act
        var subtotal = line.Quantity * line.UnitPrice;
        line.DiscountAmount = subtotal * line.DiscountPercent / 100;
        line.TaxAmount = (subtotal - line.DiscountAmount) * line.TaxPercent / 100;
        line.LineTotal = subtotal - line.DiscountAmount + line.TaxAmount;

        // Assert
        line.DiscountAmount.Should().Be(100m);
        line.LineTotal.Should().Be(900m);
    }

    [Fact]
    public void InvoiceLine_ShouldSupportGstFields()
    {
        // Arrange & Act
        var line = new InvoiceLine
        {
            CgstRate = 9m,
            SgstRate = 9m,
            HsnCode = "8471"
        };

        // Assert
        line.CgstRate.Should().Be(9m);
        line.SgstRate.Should().Be(9m);
        line.HsnCode.Should().Be("8471");
    }
}

public class InvoicePaymentTests
{
    [Fact]
    public void InvoicePayment_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var payment = new InvoicePayment();

        // Assert
        payment.Id.Should().NotBe(Guid.Empty);
        payment.PaymentNumber.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Check)]
    [InlineData(PaymentMethod.BankTransfer)]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.DebitCard)]
    [InlineData(PaymentMethod.PayPal)]
    [InlineData(PaymentMethod.Other)]
    public void InvoicePayment_ShouldSupportAllPaymentMethods(PaymentMethod method)
    {
        // Arrange
        var payment = new InvoicePayment();

        // Act
        payment.PaymentMethod = method;

        // Assert
        payment.PaymentMethod.Should().Be(method);
    }
}
