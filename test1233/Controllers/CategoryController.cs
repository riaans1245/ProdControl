using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 5;

    public IActionResult Index(int page = 1)
    {
        var categories = _userStore.GetAllCategories();
        var totalItems = categories.Count;
        var pageNumber = Math.Max(1, page);
        var items = categories
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppCategory>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_userStore.CategoryNameExists(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "That category already exists.");
            return View(model);
        }

        _userStore.CreateCategory(new AppCategory
        {
            Name = model.Name.Trim()
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var category = _userStore.GetCategoryById(id);
        if (category is null)
        {
            return NotFound();
        }

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingCategory = _userStore.GetCategoryById(model.Id);
        if (existingCategory is null)
        {
            return NotFound();
        }

        if (_userStore.CategoryNameExists(model.Name, model.Id))
        {
            ModelState.AddModelError(nameof(model.Name), "That category already exists.");
            return View(model);
        }

        _userStore.UpdateCategory(new AppCategory
        {
            Id = model.Id,
            Name = model.Name.Trim()
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var category = _userStore.GetCategoryById(id);
        if (category is null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var category = _userStore.GetCategoryById(id);
        if (category is null)
        {
            return NotFound();
        }

        _userStore.DeleteCategory(id);
        return RedirectToAction(nameof(Index));
    }
}
