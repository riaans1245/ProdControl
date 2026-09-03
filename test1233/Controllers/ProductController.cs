using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, string sortOrder = "", int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["ProductSort"] = sortOrder == "product_asc" ? "product_desc" : "product_asc";
        ViewData["PriceSort"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
        ViewData["CategorySort"] = sortOrder == "category_asc" ? "category_desc" : "category_asc";

        var product = from u in _userStore.GetAllProducts()
                    select u;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            product = product.Where(u =>
                u.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                u.CategoryName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        product = sortOrder switch
        {
            "product_asc" => product
                .OrderBy(u => u.Name)
                .ThenBy(u => u.CategoryName)
                .ThenBy(u => u.Id),
            "product_desc" => product
                .OrderByDescending(u => u.Name)
                .ThenBy(u => u.CategoryName)
                .ThenBy(u => u.Id),
            "price_asc" => product
                .OrderBy(u => u.Price)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "price_desc" => product
                .OrderByDescending(u => u.Price)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "category_asc" => product
                .OrderBy(u => u.CategoryName)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "category_desc" => product
                .OrderByDescending(u => u.CategoryName)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            _ => product
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Id)
        };

        var filteredUsers = product.ToList();
        var totalItems = filteredUsers.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredUsers
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppProduct>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }


    public IActionResult Create()
    {
        return View(new ProductFormViewModel
        {
            AvailableCategories = GetCategorySelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetCategorySelectList();
            return View(model);
        }

        var category = _userStore.GetCategoryById(model.CategoryId);
        if (category is null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Please choose a valid category.");
        }

        if (_userStore.ProductNameExists(model.Name, model.CategoryId))
        {
            ModelState.AddModelError(nameof(model.Name), "That product already exists in the selected category.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetCategorySelectList();
            return View(model);
        }

        _userStore.CreateProduct(new AppProduct
        {
            Name = model.Name.Trim(),
            Price = model.Price,
            CategoryId = category!.Id,
            CategoryName = category.Name
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var product = _userStore.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            AvailableCategories = GetCategorySelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetCategorySelectList();
            return View(model);
        }

        var existingProduct = _userStore.GetProductById(model.Id);
        if (existingProduct is null)
        {
            return NotFound();
        }

        var category = _userStore.GetCategoryById(model.CategoryId);
        if (category is null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Please choose a valid category.");
        }

        if (_userStore.ProductNameExists(model.Name, model.CategoryId, model.Id))
        {
            ModelState.AddModelError(nameof(model.Name), "That product already exists in the selected category.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableCategories = GetCategorySelectList();
            return View(model);
        }

        _userStore.UpdateProduct(new AppProduct
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            Price = model.Price,
            CategoryId = category!.Id,
            CategoryName = category.Name
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var product = _userStore.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = _userStore.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        _userStore.DeleteProduct(id);
        return RedirectToAction(nameof(Index));
    }

    private IReadOnlyCollection<SelectListItem> GetCategorySelectList()
    {
        return _userStore.GetAllCategories()
            .Select(category => new SelectListItem(category.Name, category.Id.ToString()))
            .ToList();
    }
}
