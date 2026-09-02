namespace test1233.Models;

public class SalesReportViewModel
{
    public PagedListViewModel<SalesReportRowViewModel> Orders { get; init; } = new();

    public string SearchString { get; init; } = string.Empty;

    public int TotalItemCount { get; init; }

    public decimal TotalSalesValue { get; init; }
}

public class SalesReportRowViewModel
{
    public int OrderId { get; init; }

    public int UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public DateTime PlacedAtUtc { get; init; }

    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal Price { get; init; }

    public bool IsPaid { get; init; }

    public decimal LineTotal => Quantity * Price;
}
