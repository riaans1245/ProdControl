using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;


public class ProductIngredientUsersController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index(int id)
    {
        var product = _userStore.GetProductById(id);
        if (product is null)
        {
            return NotFound();
        }

        var ingredient = _userStore.GetAllProductIngredience()
            .Where(item =>
                item.ProductId == product.Id ||
                string.Equals(item.Name, product.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.ProductId == product.Id)
            .ThenByDescending(item => !string.IsNullOrWhiteSpace(item.ProdIngredienceName))
            .FirstOrDefault();

        var model = new ProductIngredientDetailsViewModel
        {
            ProductName = product.Name,
            ProdIngredienceName = ingredient?.ProdIngredienceName ?? string.Empty
        };

        return View(model);
    }
}
