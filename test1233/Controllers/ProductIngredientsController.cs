using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;


public class ProductIngredientsController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index(int id)
    {
        var product = _userStore.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        var ingredient = _userStore.GetProductIngredienceById(id);
        var items = ingredient is null
            ? Array.Empty<AppProductIngredience>()
            : [ingredient];

        ViewData["ProductName"] = product.Name;

        return View(new PagedListViewModel<AppProductIngredience>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 1,
            TotalItems = items.Length
        });
    }
}
