using Microsoft.AspNetCore.Mvc.Rendering;
namespace test1233.Models;
public class AppTokens
{
    public int TokenId { get; set; }

    public required string Token { get; set; }

     public int UserId { get; set; }

    public required string Username { get; set; }
}