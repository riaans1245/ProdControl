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
                token.ProductId,
                token.ProductName,
                token.SentAtUtc
            });

        return Ok(tokens);
    }

    [HttpPost("use")]
    public IActionResult UseToken([FromBody] UseTokenApiRequest request)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { success = false, message = "You must be signed in to send token data." });
        }

        if (request is null)
        {
            return BadRequest(new { success = false, message = "A JSON request body is required." });
        }

        var token = _userStore.GetTokenById(request.TokenId);
        if (token is null || token.UserId != currentUser.Id)
        {
            return NotFound(new { success = false, message = "Token not found." });
        }

        if (token.UserId != request.UserId ||
            !string.Equals(token.Username, request.Username, StringComparison.Ordinal) ||
            !string.Equals(token.Token, request.Token, StringComparison.Ordinal))
        {
            return BadRequest(new { success = false, message = "The JSON token payload does not match the stored token." });
        }

        _userStore.RecordUsedToken(new AppUsedToken
        {
            TokenId = token.TokenId,
            Token = token.Token,
            UserId = token.UserId,
            Username = token.Username,
            ProductId = token.ProductId,
            ProductName = token.ProductName,
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
