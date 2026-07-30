using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class HomeController(IUserStore userStore, IWebHostEnvironment environment) : Controller
{
    private readonly IUserStore _userStore = userStore;
    private readonly IWebHostEnvironment _environment = environment;

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

    public IActionResult Menu()
    {
        return View(BuildManageMenuViewModel());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult ManageMenu()
    {
        return View(BuildManageMenuViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageMenu(MenuUploadViewModel model)
    {
        if (model.MenuFile is null || model.MenuFile.Length == 0)
        {
            ModelState.AddModelError(nameof(model.MenuFile), "Please choose a JPG menu image to upload.");
        }
        else
        {
            var extension = Path.GetExtension(model.MenuFile.FileName);
            var allowedExtensions = new[] { ".jpg", ".jpeg" };

            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.MenuFile), "Only JPG menu images are allowed.");
            }
        }

        if (!ModelState.IsValid)
        {
            model = BuildManageMenuViewModel(model);
            return View(model);
        }

        var imagesFolder = Path.Combine(_environment.WebRootPath, "images");
        var menuHistoryFolder = Path.Combine(imagesFolder, "menu-history");
        Directory.CreateDirectory(imagesFolder);
        Directory.CreateDirectory(menuHistoryFolder);

        var historyFileName = $"Menu-{DateTime.UtcNow:yyyyMMddHHmmssfff}.jpg";
        var historyImagePath = Path.Combine(menuHistoryFolder, historyFileName);

        await using var memoryStream = new MemoryStream();
        await model.MenuFile!.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        await System.IO.File.WriteAllBytesAsync(historyImagePath, fileBytes);

        TempData["MenuUploadSuccess"] = "Menu image uploaded successfully.";

        return RedirectToAction(nameof(ManageMenu));
    }

    private MenuUploadViewModel BuildManageMenuViewModel()
    {
        return BuildManageMenuViewModel(new MenuUploadViewModel());
    }

    private MenuUploadViewModel BuildManageMenuViewModel(MenuUploadViewModel model)
    {
        var menus = GetMenus();
        model.LatestMenu = menus.FirstOrDefault();
        model.Menus = menus;
        return model;
    }

    private IReadOnlyList<MenuListItemViewModel> GetMenus()
    {
        var imagesFolder = Path.Combine(_environment.WebRootPath, "images");
        var menuHistoryFolder = Path.Combine(imagesFolder, "menu-history");
        var menus = new List<MenuListItemViewModel>();

        if (Directory.Exists(menuHistoryFolder))
        {
            menus.AddRange(Directory.GetFiles(menuHistoryFolder, "Menu-*.jpg")
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.CreationTimeUtc)
                .Select(file => new MenuListItemViewModel
                {
                    Path = $"/images/menu-history/{file.Name}",
                    UploadedAtUtc = file.CreationTimeUtc
                }));
        }

        var legacyMenuPath = Path.Combine(imagesFolder, "Menu.jpg");
        if (System.IO.File.Exists(legacyMenuPath))
        {
            const string publicLegacyMenuPath = "/images/Menu.jpg";
            if (!menus.Any(menu => string.Equals(menu.Path, publicLegacyMenuPath, StringComparison.OrdinalIgnoreCase)))
            {
                var legacyFile = new FileInfo(legacyMenuPath);
                menus.Add(new MenuListItemViewModel
                {
                    Path = publicLegacyMenuPath,
                    UploadedAtUtc = legacyFile.CreationTimeUtc
                });
            }
        }

        return menus
            .OrderByDescending(menu => menu.UploadedAtUtc)
            .ToList()
            .AsReadOnly();
    }

}
