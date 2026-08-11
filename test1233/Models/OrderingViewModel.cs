namespace test1233.Models;

public class OrderingViewModel
{
    public IReadOnlyCollection<AppProduct> Products { get; init; } = Array.Empty<AppProduct>();

    public IReadOnlyCollection<AppCartItem> CartItems { get; init; } = Array.Empty<AppCartItem>();

    public int CartItemCount => CartItems.Sum(item => item.Quantity);

    public decimal CartTotalValue => CartItems.Sum(item => item.Quantity * item.Price);
}
