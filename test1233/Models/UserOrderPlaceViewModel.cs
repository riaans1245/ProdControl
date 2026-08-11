namespace test1233.Models;

public class UserOrderPlaceViewModel
{
    public IReadOnlyCollection<AppOrder> PendingOrders { get; init; } = Array.Empty<AppOrder>();

    public AppReceipt? LatestReceipt { get; init; }

    public int TotalItems => PendingOrders
        .SelectMany(order => order.Items)
        .Sum(item => item.Quantity);

    public decimal TotalValue => PendingOrders
        .SelectMany(order => order.Items)
        .Sum(item => item.Quantity * item.Price);
}
