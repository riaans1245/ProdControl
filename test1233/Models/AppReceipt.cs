namespace test1233.Models;

public class AppReceipt
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime PaidAtUtc { get; set; }

    public List<int> OrderIds { get; set; } = [];

    public List<AppReceiptItem> Items { get; set; } = [];

    public List<AppReceiptAppliedToken> AppliedTokens { get; set; } = [];
}

public class AppReceiptItem
{
    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}

public class AppReceiptAppliedToken
{
    public int OrderId { get; set; }

    public int TokenId { get; set; }

    public string Token { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal DiscountAmount { get; set; }
}
