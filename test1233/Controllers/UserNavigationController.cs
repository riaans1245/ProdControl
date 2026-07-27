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
        return View(_userStore.GetAllTokens());
    }

    public IActionResult UserNotificationsList()
    {
        return View(_userStore.GetAllNotifications());
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
}
