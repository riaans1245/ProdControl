using System;
using System.Collections.Generic;
using System.Linq;

public sealed class InvoiceLine
{
    public InvoiceLine(string description, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("A description is required.", nameof(description));
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "An invoice line amount cannot be negative.");
        }

        Description = description;
        Amount = amount;
    }

    public string Description { get; }
    public decimal Amount { get; }
}

public interface IInvoice
{
    IReadOnlyCollection<InvoiceLine> Lines { get; }
    decimal Subtotal { get; }
    decimal DiscountRate { get; }
    decimal Total { get; }

    void AddLine(string description, decimal amount);
}

public abstract class Invoice : IInvoice
{
    private readonly List<InvoiceLine> _lines = new();

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();
    public decimal Subtotal => _lines.Sum(line => line.Amount);
    public virtual decimal DiscountRate => 0m;
    public decimal Total => Subtotal * (1m - DiscountRate);

    public void AddLine(string description, decimal amount)
    {
        _lines.Add(new InvoiceLine(description, amount));
    }
}

public interface IPromoCodeDiscountProvider
{
    decimal GetDiscountRate(string promoCode);
}

public sealed class PromoCodeDiscountProvider : IPromoCodeDiscountProvider
{
    private static readonly IReadOnlyDictionary<string, decimal> DiscountRates =
        new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["TENER"] = 0.10m,
            ["MATESRATES"] = 0.20m,
            ["IKNOWTHEOWNER"] = 0.50m
        };

    public decimal GetDiscountRate(string promoCode)
    {
        if (string.IsNullOrWhiteSpace(promoCode))
        {
            return 0m;
        }

        return DiscountRates.TryGetValue(promoCode, out var rate) ? rate : 0m;
    }
}

public sealed class IncomeInvoice : Invoice
{
    private readonly IPromoCodeDiscountProvider _discountProvider;
    private decimal _discountRate;

    public IncomeInvoice(IPromoCodeDiscountProvider discountProvider)
    {
        _discountProvider = discountProvider ?? throw new ArgumentNullException(nameof(discountProvider));
    }

    public override decimal DiscountRate => _discountRate;

    public void ApplyDiscount(string promoCode)
    {
        _discountRate = _discountProvider.GetDiscountRate(promoCode);
    }
}

// Expense invoices do not expose discount behaviour, so they remain substitutable as IInvoice.
public sealed class ExpenseInvoice : Invoice
{
}

public interface IInvoiceEmailSender
{
    void Send(IInvoice invoice, string emailAddress);
}

public sealed class ConsoleInvoiceEmailSender : IInvoiceEmailSender
{
    public void Send(IInvoice invoice, string emailAddress)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            throw new ArgumentException("An email address is required.", nameof(emailAddress));
        }

        Console.WriteLine($"Emailed {invoice.GetType().Name} of ${invoice.Total} to {emailAddress}");
    }
}

public interface IInvoiceExporter
{
    void Export(IInvoice invoice);
}

public sealed class CsvInvoiceExporter : IInvoiceExporter
{
    public void Export(IInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        Console.WriteLine($"Exported {invoice.GetType().Name} of ${invoice.Total} to CSV");
    }
}

public sealed class PdfInvoiceExporter : IInvoiceExporter
{
    public void Export(IInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        Console.WriteLine($"Exported {invoice.GetType().Name} of ${invoice.Total} to PDF");
    }
}

public static class Program
{
    public static void Main()
    {
        var emailSender = new ConsoleInvoiceEmailSender();
        var csvExporter = new CsvInvoiceExporter();
        var pdfExporter = new PdfInvoiceExporter();

        var incomeInvoice = new IncomeInvoice(new PromoCodeDiscountProvider());
        incomeInvoice.AddLine("Apple", 2m);
        incomeInvoice.AddLine("Banana", 3m);
        incomeInvoice.AddLine("Carrot", 1m);
        incomeInvoice.ApplyDiscount("TENER");
        emailSender.Send(incomeInvoice, "income@abc.com");

        var expenseInvoice = new ExpenseInvoice();
        expenseInvoice.AddLine("Rent", 1000m);
        emailSender.Send(expenseInvoice, "expense@abc.com");

        csvExporter.Export(incomeInvoice);
        pdfExporter.Export(expenseInvoice);
    }
}
