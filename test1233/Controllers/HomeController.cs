using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class HomeController(IUserStore userStore) : Controller
{
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
        return View(new ContactUs
        {
            Name = string.Empty,
            Surname = string.Empty,
            EmailAddress = string.Empty,
            CellNo = string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ContactUs(ContactUs model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _userStore.ContactUs(model);
        TempData["ContactSuccess"] = "Thanks for reaching out. The Easy Eats team will contact you soon.";

        return RedirectToAction(nameof(ContactUs));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
