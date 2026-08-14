namespace test1233.Models;

public class AppUsedToken
{
    public int Id { get; set; }

    public int TokenId { get; set; }

    public required string Token { get; set; }

    public int UserId { get; set; }

    public required string Username { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public DateTime SentAtUtc { get; set; }
}
