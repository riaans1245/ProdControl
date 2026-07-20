using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class AccountController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated is true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _userStore.ValidateUser(model.Username, model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        if (model.RequestMagicLink)
        {
            var token = _userStore.CreateMagicLink(user.EmailAddress);
            var magicLinkUrl = Url.Action(nameof(MagicLinkLogin), "Account", new { token }, Request.Scheme);

            TempData["MagicLinkEmail"] = user.EmailAddress;
            TempData["MagicLinkUrl"] = magicLinkUrl;
        }

        await SignInUserAsync(user, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_userStore.UsernameExists(model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "That username is already registered.");
            return View(model);
        }

        if (_userStore.EmailAddressExists(model.EmailAddress))
        {
            ModelState.AddModelError(nameof(model.EmailAddress), "That email address is already registered.");
            return View(model);
        }

        var defaultRole = _userStore.GetRoleById(2);
        if (defaultRole is null)
        {
            ModelState.AddModelError(string.Empty, "The default Users role could not be found.");
            return View(model);
        }

        var user = new AppUser
        {
            Username = model.Username.Trim(),
            Name = model.Name.Trim(),
            Surname = model.Surname.Trim(),
            EmailAddress = model.EmailAddress.Trim(),
            CellNo = model.CellNo.Trim(),
            Password = model.Password,
            RoleId = defaultRole.Id,
            Role = defaultRole.Name
        };

        _userStore.CreateUser(user);
        await SignInUserAsync(user, false);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> MagicLinkLogin(string token, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["MagicLinkError"] = "The magic link is missing or invalid.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var user = _userStore.ConsumeMagicLink(token);
        if (user is null)
        {
            TempData["MagicLinkError"] = "This magic link is invalid or has already been used.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        await SignInUserAsync(user, false);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
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
