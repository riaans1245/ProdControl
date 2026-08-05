namespace test1233.Models;

public class OrderPaymentRequest
{
    public string ReceiptNumber { get; set; } = string.Empty;

    public string PaidAtUtc { get; set; } = string.Empty;

    public int TotalItems { get; set; }

    public decimal TotalValue { get; set; }

    public IReadOnlyList<int> OrderIds { get; set; } = Array.Empty<int>();

    public IReadOnlyList<OrderPaymentItemRequest> Items { get; set; } = Array.Empty<OrderPaymentItemRequest>();
}

public class OrderPaymentItemRequest
{
    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
