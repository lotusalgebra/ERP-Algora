using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Algora.Erp.Infrastructure.Services;

public class InvoicePdfService : IInvoicePdfService
{
    // Currency formatting based on invoice currency
    private string FormatCurrency(decimal amount, string? currency)
    {
        return currency?.ToUpperInvariant() switch
        {
            "INR" => $"₹{amount:N2}",
            "USD" => $"${amount:N2}",
            "EUR" => $"€{amount:N2}",
            "GBP" => $"£{amount:N2}",
            _ => $"₹{amount:N2}" // Default to INR
        };
    }

    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(c => ComposeFooter(c, invoice));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Algora ERP").Bold().FontSize(24).FontColor(Colors.Blue.Darken3);
                    col.Item().Text("123 Business Street");
                    col.Item().Text("City, State 12345");
                    col.Item().Text("Phone: (555) 123-4567");
                    col.Item().Text("Email: invoices@algora-erp.com");
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("INVOICE").Bold().FontSize(28).FontColor(Colors.Grey.Darken2);
                    col.Item().AlignRight().Text(invoice.InvoiceNumber).FontSize(14);
                    col.Item().AlignRight().PaddingTop(10).Element(c => ComposeStatusBadge(c, invoice));
                });
            });

            column.Item().PaddingTop(20).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
        });
    }

    private void ComposeStatusBadge(IContainer container, Invoice invoice)
    {
        var (text, bgColor, textColor) = invoice.Status switch
        {
            InvoiceStatus.Draft => ("DRAFT", Colors.Grey.Lighten2, Colors.Grey.Darken3),
            InvoiceStatus.Pending => ("PENDING", Colors.Yellow.Lighten2, Colors.Yellow.Darken4),
            InvoiceStatus.Sent => ("AWAITING PAYMENT", Colors.Blue.Lighten3, Colors.Blue.Darken3),
            InvoiceStatus.PartiallyPaid => ("PARTIALLY PAID", Colors.Green.Lighten3, Colors.Green.Darken3),
            InvoiceStatus.Paid => ("PAID", Colors.Green.Darken1, Colors.White),
            InvoiceStatus.Overdue => ("OVERDUE", Colors.Red.Lighten3, Colors.Red.Darken3),
            InvoiceStatus.Void => ("VOID", Colors.Grey.Darken2, Colors.White),
            InvoiceStatus.Cancelled => ("CANCELLED", Colors.Grey.Medium, Colors.White),
            _ => ("", Colors.Grey.Lighten2, Colors.Grey.Darken3)
        };

        container.Background(bgColor).Padding(5).Text(text).Bold().FontSize(9).FontColor(textColor);
    }

    private void ComposeContent(IContainer container, Invoice invoice)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Bill To and Invoice Details
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILL TO").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5);

                    if (invoice.Customer != null)
                    {
                        col.Item().Text(invoice.Customer.Name).Bold().FontSize(12);
                    }
                    else if (!string.IsNullOrEmpty(invoice.BillingName))
                    {
                        col.Item().Text(invoice.BillingName).Bold().FontSize(12);
                    }

                    if (!string.IsNullOrEmpty(invoice.BillingAddress))
                        col.Item().Text(invoice.BillingAddress);

                    var cityStateZip = new List<string>();
                    if (!string.IsNullOrEmpty(invoice.BillingCity))
                        cityStateZip.Add(invoice.BillingCity);
                    if (!string.IsNullOrEmpty(invoice.BillingState))
                        cityStateZip.Add(invoice.BillingState);
                    if (!string.IsNullOrEmpty(invoice.BillingPostalCode))
                        cityStateZip.Add(invoice.BillingPostalCode);
                    if (cityStateZip.Any())
                        col.Item().Text(string.Join(", ", cityStateZip));

                    if (!string.IsNullOrEmpty(invoice.BillingCountry))
                        col.Item().Text(invoice.BillingCountry);

                    if (invoice.Customer?.Email != null)
                        col.Item().PaddingTop(5).Text(invoice.Customer.Email).FontColor(Colors.Blue.Darken2);
                });

                row.ConstantItem(200).Column(col =>
                {
                    col.Item().Text("INVOICE DETAILS").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(5);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Invoice Number:");
                        r.ConstantItem(100).AlignRight().Text(invoice.InvoiceNumber).Bold();
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Invoice Date:");
                        r.ConstantItem(100).AlignRight().Text(invoice.InvoiceDate.ToString("MMM dd, yyyy"));
                    });

                    var isOverdue = invoice.DueDate < DateTime.UtcNow &&
                        invoice.Status != InvoiceStatus.Paid &&
                        invoice.Status != InvoiceStatus.Void;

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Due Date:");
                        r.ConstantItem(100).AlignRight().Text(invoice.DueDate.ToString("MMM dd, yyyy"))
                            .FontColor(isOverdue ? Colors.Red.Darken2 : Colors.Black);
                    });

                    if (!string.IsNullOrEmpty(invoice.PaymentTerms))
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Payment Terms:");
                            r.ConstantItem(100).AlignRight().Text(invoice.PaymentTerms);
                        });
                    }

                    if (!string.IsNullOrEmpty(invoice.Reference))
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Reference:");
                            r.ConstantItem(100).AlignRight().Text(invoice.Reference);
                        });
                    }

                    if (invoice.SalesOrder != null)
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Sales Order:");
                            r.ConstantItem(100).AlignRight().Text(invoice.SalesOrder.OrderNumber);
                        });
                    }
                });
            });

            // Line Items Table
            column.Item().PaddingTop(30).Element(c => ComposeLineItemsTable(c, invoice));

            // Totals
            column.Item().PaddingTop(10).Element(c => ComposeTotals(c, invoice));

            // Payment History
            if (invoice.Payments?.Any() == true)
            {
                column.Item().PaddingTop(20).Element(c => ComposePaymentHistory(c, invoice));
            }

            // Notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().PaddingTop(20).Element(c => ComposeNotes(c, invoice));
            }
        });
    }

    private void ComposeLineItemsTable(IContainer container, Invoice invoice)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);   // #
                columns.RelativeColumn(3);    // Description
                columns.ConstantColumn(60);   // HSN
                columns.ConstantColumn(40);   // Qty
                columns.ConstantColumn(65);   // Unit Price
                columns.ConstantColumn(70);   // GST
                columns.ConstantColumn(70);   // Tax Amt
                columns.ConstantColumn(75);   // Amount
            });

            // Header - show appropriate tax column based on transaction type
            var taxHeader = invoice.IsInterState ? "IGST" : "CGST/SGST";
            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("#").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Description").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignCenter().Text("HSN").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignCenter().Text("Qty").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignRight().Text("Rate").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignCenter().Text(taxHeader).FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignRight().Text("Tax Amt").FontColor(Colors.White).Bold().FontSize(8);
                header.Cell().Background(Colors.Blue.Darken3).Padding(6).AlignRight().Text("Amount").FontColor(Colors.White).Bold().FontSize(8);
            });

            // Rows
            var lines = invoice.Lines?.OrderBy(l => l.LineNumber).ToList() ?? new List<InvoiceLine>();
            foreach (var line in lines)
            {
                var bgColor = lines.IndexOf(line) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                // Line number
                table.Cell().Background(bgColor).Padding(6).Text(line.LineNumber.ToString()).FontSize(9);

                // Description with SKU
                table.Cell().Background(bgColor).Padding(6).Column(col =>
                {
                    col.Item().Text(line.Description).Bold().FontSize(9);
                    if (!string.IsNullOrEmpty(line.ProductCode))
                        col.Item().Text($"SKU: {line.ProductCode}").FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                // HSN Code
                table.Cell().Background(bgColor).Padding(6).AlignCenter().Text(line.HsnCode ?? "-").FontSize(9);

                // Quantity
                table.Cell().Background(bgColor).Padding(6).AlignCenter().Text(line.Quantity.ToString("0.##")).FontSize(9);

                // Unit Price
                table.Cell().Background(bgColor).Padding(6).AlignRight().Text(FormatCurrency(line.UnitPrice, invoice.Currency)).FontSize(9);

                // GST Rate - show IGST for inter-state, CGST+SGST for intra-state
                string gstRateText;
                if (invoice.IsInterState && line.IgstRate > 0)
                {
                    gstRateText = $"{line.IgstRate:0.##}%";
                }
                else if (line.CgstRate > 0 || line.SgstRate > 0)
                {
                    gstRateText = $"{line.CgstRate:0.##}%+{line.SgstRate:0.##}%";
                }
                else if (line.TaxPercent > 0)
                {
                    gstRateText = $"{line.TaxPercent:0.##}%";
                }
                else
                {
                    gstRateText = "-";
                }
                table.Cell().Background(bgColor).Padding(6).AlignCenter().Text(gstRateText).FontSize(9);

                // Tax Amount
                table.Cell().Background(bgColor).Padding(6).AlignRight().Text(FormatCurrency(line.TaxAmount, invoice.Currency)).FontSize(9);

                // Line Total
                table.Cell().Background(bgColor).Padding(6).AlignRight().Text(FormatCurrency(line.LineTotal, invoice.Currency)).Bold().FontSize(9);
            }
        });
    }

    private void ComposeTotals(IContainer container, Invoice invoice)
    {
        var currency = invoice.Currency;

        container.AlignRight().Width(250).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().AlignRight().Text("Subtotal:");
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.SubTotal, currency));
            });

            if (invoice.DiscountAmount > 0)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Discount:").FontColor(Colors.Red.Darken2);
                    row.ConstantItem(100).AlignRight().Text($"-{FormatCurrency(invoice.DiscountAmount, currency)}").FontColor(Colors.Red.Darken2);
                });
            }

            // GST Breakdown - show CGST/SGST for intra-state, IGST for inter-state
            if (invoice.IsInterState && invoice.IgstAmount > 0)
            {
                // Inter-state: Show IGST only
                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("IGST:");
                    row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.IgstAmount, currency));
                });
            }
            else if (invoice.CgstAmount > 0 || invoice.SgstAmount > 0)
            {
                // Intra-state: Show CGST and SGST separately
                if (invoice.CgstAmount > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text("CGST:");
                        row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.CgstAmount, currency));
                    });
                }
                if (invoice.SgstAmount > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Text("SGST:");
                        row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.SgstAmount, currency));
                    });
                }
            }
            else if (invoice.TaxAmount > 0)
            {
                // Fallback: Show generic tax if no GST breakdown
                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Tax:");
                    row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.TaxAmount, currency));
                });
            }

            column.Item().PaddingTop(5).BorderTop(2).BorderColor(Colors.Blue.Darken3).PaddingTop(5).Row(row =>
            {
                row.RelativeItem().AlignRight().Text("Total:").Bold().FontSize(14);
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.TotalAmount, currency)).Bold().FontSize(14);
            });

            if (invoice.PaidAmount > 0)
            {
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Amount Paid:");
                    row.ConstantItem(100).AlignRight().Text($"-{FormatCurrency(invoice.PaidAmount, currency)}").FontColor(Colors.Green.Darken2);
                });
            }

            column.Item().PaddingTop(5).Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
            {
                row.RelativeItem().AlignRight().Text("Balance Due:").Bold();
                row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.BalanceDue, currency)).Bold()
                    .FontColor(invoice.BalanceDue > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
            });
        });
    }

    private void ComposePaymentHistory(IContainer container, Invoice invoice)
    {
        container.Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
        {
            column.Item().Text("PAYMENT HISTORY").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
            column.Item().PaddingTop(10);

            foreach (var payment in invoice.Payments.OrderBy(p => p.PaymentDate))
            {
                column.Item().PaddingBottom(5).Row(row =>
                {
                    row.ConstantItem(100).Text(payment.PaymentNumber);
                    row.ConstantItem(100).Text(payment.PaymentDate.ToString("MMM dd, yyyy"));
                    row.ConstantItem(100).Text(payment.PaymentMethod.ToString());
                    row.RelativeItem().Text(payment.Reference ?? "-");
                    row.ConstantItem(80).AlignRight().Text(FormatCurrency(payment.Amount, invoice.Currency)).FontColor(Colors.Green.Darken2).Bold();
                });
            }
        });
    }

    private void ComposeNotes(IContainer container, Invoice invoice)
    {
        container.Background(Colors.Yellow.Lighten4).BorderLeft(3).BorderColor(Colors.Yellow.Darken2).Padding(15).Column(column =>
        {
            column.Item().Text("NOTES").Bold().FontSize(9).FontColor(Colors.Yellow.Darken4);
            column.Item().PaddingTop(5).Text(invoice.Notes);
        });
    }

    private void ComposeFooter(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(15);

            column.Item().AlignCenter().Text("Thank you for your business!").Bold();
            column.Item().AlignCenter().Text($"Payment is due by {invoice.DueDate:MMMM dd, yyyy}").FontSize(9);
            column.Item().AlignCenter().Text($"Please include invoice number {invoice.InvoiceNumber} with your payment").FontSize(9).FontColor(Colors.Grey.Darken1);

            if (!string.IsNullOrEmpty(invoice.PaymentTerms))
            {
                column.Item().AlignCenter().PaddingTop(5).Text($"Terms: {invoice.PaymentTerms}").FontSize(9).FontColor(Colors.Grey.Darken1);
            }
        });
    }
}
