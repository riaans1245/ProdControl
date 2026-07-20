using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class HomeController(ILogger<HomeController> logger, IUserStore userStore) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public IActionResult Ordering()
    {
        var products = _userStore.GetAllProducts()
            .OrderBy(product => product.CategoryName)
            .ThenBy(product => product.Name)
            .ToList()
            .AsReadOnly();

        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    public IActionResult Private()
    {
        return View(_userStore.GetAllUsers());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
