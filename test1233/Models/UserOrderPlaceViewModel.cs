namespace test1233.Models;

public class UserOrderPlaceViewModel
{
    public IReadOnlyCollection<AppOrder> PendingOrders { get; init; } = Array.Empty<AppOrder>();

    public AppReceipt? LatestReceipt { get; init; }

    public IReadOnlyCollection<OrderPaymentTokenOptionViewModel> EligibleTokens { get; init; } = Array.Empty<OrderPaymentTokenOptionViewModel>();

    public IReadOnlyCollection<int> SelectedTokenIds { get; init; } = Array.Empty<int>();

    public int TotalItems => PendingOrders
        .SelectMany(order => order.Items)
        .Sum(item => item.Quantity);

    public decimal TotalValue => PendingOrders
        .SelectMany(order => order.Items)
        .Sum(item => item.Quantity * item.Price);

    public decimal SelectedTokenDiscount => EligibleTokens
        .Where(token => SelectedTokenIds.Contains(token.TokenId))
        .Sum(token => token.DiscountAmount);

    public decimal TotalDue => Math.Max(0m, TotalValue - SelectedTokenDiscount);
}

public class OrderPaymentTokenOptionViewModel
{
    public int TokenId { get; init; }

    public string Token { get; init; } = string.Empty;

    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal DiscountAmount { get; init; }

    public int OrderId { get; init; }
}
