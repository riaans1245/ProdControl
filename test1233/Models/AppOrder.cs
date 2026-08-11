namespace test1233.Models;

public class AppOrder
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public DateTime PlacedAtUtc { get; set; }

    public bool IsPaid { get; set; }

    public List<AppOrderItem> Items { get; set; } = [];
}

public class AppOrderItem
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}
