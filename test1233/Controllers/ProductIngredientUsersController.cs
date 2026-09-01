using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;


public class ProductIngredientUsersController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;

        var productIngredients = from item in _userStore.GetAllProductIngredience()
                                 select item;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            productIngredients = productIngredients.Where(item =>
                item.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                item.ProdIngredienceName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        var filteredItems = productIngredients.ToList();
        var totalItems = filteredItems.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredItems
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppProductIngredience>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }
}
