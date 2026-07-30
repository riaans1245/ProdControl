using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;


public class UserNavigationController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserTokenList()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var tokens = _userStore.GetAllTokens()
            .Where(token => token.UserId == currentUser.Id)
            .ToList()
            .AsReadOnly();

        return View(tokens);
    }

    public IActionResult UserNotificationsList()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var notifications = _userStore.GetAllNotifications()
            .Where(notification => notification.UserId == currentUser.Id)
            .ToList()
            .AsReadOnly();

        return View(notifications);
    }

    public IActionResult UserOrderList()
    {
        return View();
    }

    public IActionResult Products()
    {
        return View(_userStore.GetAllProducts());
    }

    public IActionResult Categories()
    {
        return View(_userStore.GetAllCategories());
    }

    public IActionResult ContactUs()
    {
        return View(_userStore.GetAllContactUs());
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
