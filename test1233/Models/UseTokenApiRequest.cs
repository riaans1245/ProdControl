namespace test1233.Models;

public class UseTokenApiRequest
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public required string Username { get; set; }

    public required string Token { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
}
