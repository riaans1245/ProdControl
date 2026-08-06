using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[ApiController]
[Route("api/tokens")]
public class TokenApiController(IUserStore userStore) : ControllerBase
{
    private readonly IUserStore _userStore = userStore;

    [HttpGet("used")]
    public IActionResult GetUsedTokens()
    {
        var tokens = _userStore.GetAllUsedTokens()
            .Select(token => new
            {
                token.Id,
                token.TokenId,
                token.Token,
                token.UserId,
                token.Username,
                token.SentAtUtc
            });

        return Ok(tokens);
    }

    [HttpPost("use/{tokenId:int}")]
    public IActionResult UseToken(int tokenId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { success = false, message = "You must be signed in to send token data." });
        }

        var token = _userStore.GetTokenById(tokenId);
        if (token is null || token.UserId != currentUser.Id)
        {
            return NotFound(new { success = false, message = "Token not found." });
        }

        _userStore.RecordUsedToken(new AppUsedToken
        {
            TokenId = token.TokenId,
            Token = token.Token,
            UserId = token.UserId,
            Username = token.Username,
            SentAtUtc = DateTime.UtcNow
        });

        return Ok(new
        {
            success = true,
            message = "API sent the token to the administrator."
        });
    }

    private AppUser? GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return _userStore.GetAllUsers()
            .FirstOrDefault(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}
