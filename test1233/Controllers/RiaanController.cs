using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class RiaanController(ICalculationService calculationService, IUserStore userStore) : Controller
{
    private readonly ICalculationService _calculationService = calculationService;
    private readonly IUserStore _userStore = userStore;

    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "Eats Dashboard";
        ViewData["DashboardLoginError"] = TempData["DashboardLoginError"];
        return View();
    }

    public IActionResult Add(int left, int right)
    {
        var result = _calculationService.Add(left, right);

        return Json(new
        {
            left,
            right,
            result
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DashboardLogin(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["DashboardLoginError"] = "Please enter a valid user name and password.";
            return RedirectToAction(nameof(Index));
        }

        var user = _userStore.ValidateUser(model.Username, model.Password);
        if (user is null)
        {
            TempData["DashboardLoginError"] = "Invalid username or password.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["DashboardLoginError"] = "Administrator access is required for Eats Dashboard.";
            return RedirectToAction(nameof(Index));
        }

        await SignInUserAsync(user, model.RememberMe);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult DashboardData()
    {
        var usedTokens = _userStore.GetAllUsedTokens()
            .OrderByDescending(token => token.SentAtUtc)
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
            })
            .ToList();

        var issuedTokens = _userStore.GetAllTokens()
            .Select(token => new
            {
                token.TokenId,
                token.Token,
                token.UserId,
                token.Username,
                token.ProductId,
                token.ProductName,
                HasBeenSent = usedTokens.Any(usedToken => usedToken.TokenId == token.TokenId)
            })
            .OrderByDescending(token => token.HasBeenSent)
            .ThenBy(token => token.Username)
            .ToList();

        return Json(new
        {
            generatedAtUtc = DateTime.UtcNow,
            currentUser = User.Identity?.Name ?? string.Empty,
            summary = new
            {
                issuedTokenCount = issuedTokens.Count,
                usedTokenCount = usedTokens.Count(),
                uniqueUsersWithSentTokens = usedTokens
                    .Select(token => token.UserId)
                    .Distinct()
                    .Count(),
                latestTokenSentAtUtc = usedTokens.FirstOrDefault()?.SentAtUtc
            },
            activityEndpoint = Url.Action("GetUsedTokens", "TokenApi"),
            issuedTokens,
            usedTokens,
            latestUsedToken = usedTokens.FirstOrDefault()
        });
    }

    private async Task SignInUserAsync(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent
            });
    }
}
