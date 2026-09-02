namespace test1233.Models;

public class UserOrderListViewModel
{
    public PagedListViewModel<UserOrderRowViewModel> Orders { get; init; } = new();

    public string SearchString { get; init; } = string.Empty;

    public string SortOrder { get; init; } = string.Empty;

    public string NameSortOrder { get; init; } = "name_asc";

    public string CategorySortOrder { get; init; } = "category_asc";

    public int PendingOrderCount { get; init; }

    public decimal PendingOrderTotal { get; init; }

    public bool HasOrders => PendingOrderCount > 0;
}

public class UserOrderRowViewModel
{
    public int OrderId { get; init; }

    public DateTime PlacedAtUtc { get; init; }

    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal Price { get; init; }

    public decimal LineTotal => Quantity * Price;
}
