namespace test1233.Models;

public class AppCartItem
{
    public int UserId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}
