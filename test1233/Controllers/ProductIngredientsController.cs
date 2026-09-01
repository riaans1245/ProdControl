using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Controllers;


public class ProductIngredientsController(IUserStore userStore) : AppController(userStore)
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

        var items = productIngredients
            .ToList();

        var totalItems = items.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);

        ViewData["PageNumber"] = pageNumber;
        ViewData["PageSize"] = PageSize;
        ViewData["TotalItems"] = totalItems;
        ViewData["TotalPages"] = totalPages;

        var pagedItems = items
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .Select(item => new ProductIngredientListItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Name = item.Name,
                ProdIngredienceName = item.ProdIngredienceName
            })
            .ToList()
            .AsReadOnly();

        return View(pagedItems);
    }

    public IActionResult Create()
    {
        return View(new ProdIngredViewModel
        {
            ProductId = 0,
            ProdIngredienceName = string.Empty,
            AvailableProducts = GetProductIngrSelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ProdIngredViewModel model)
    {
        ModelState.Remove(nameof(model.Name));

        var selectedProduct = _userStore.GetProductById(model.ProductId);
        if (selectedProduct is null)
        {
            ModelState.AddModelError(nameof(model.ProductId), "Please choose a valid product.");
        }
        else
        {
            model.Name = selectedProduct.Name;
        }

        if (string.IsNullOrWhiteSpace(model.ProdIngredienceName))
        {
            ModelState.AddModelError(nameof(model.ProdIngredienceName), "Please enter an ingredient list.");
        }

        var existingProductIngredient = selectedProduct is null
            ? null
            : _userStore.GetAllProductIngredience()
                .FirstOrDefault(item =>
                    string.Equals(item.Name, selectedProduct.Name, StringComparison.OrdinalIgnoreCase));

        if (existingProductIngredient is not null &&
            !string.IsNullOrWhiteSpace(existingProductIngredient.ProdIngredienceName))
        {
            ModelState.AddModelError(nameof(model.ProductId), "That product already has an ingredient list.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableProducts = GetProductIngrSelectList();
            return View(model);
        }

        if (existingProductIngredient is not null)
        {
            _userStore.UpdateProductIngred(new AppProductIngredience
            {
                Id = existingProductIngredient.Id,
                ProductId = selectedProduct!.Id,
                Name = selectedProduct.Name,
                ProdIngredienceName = model.ProdIngredienceName.Trim()
            });
        }
        else
        {
            _userStore.CreateProductIngredience(new AppProductIngredience
            {
                ProductId = selectedProduct!.Id,
                Name = selectedProduct.Name,
                ProdIngredienceName = model.ProdIngredienceName.Trim()
            });
        }

        return RedirectToAction(nameof(Index));
    }


     public IActionResult Edit(int id)
    {
       var productIngr = _userStore.GetProducIngredById(id);
        if (productIngr is null)
        {
            return NotFound();
        }

        return View(new ProdIngredViewModel
        {
            Id = productIngr.Id,
            ProdIngredienceName = productIngr.ProdIngredienceName,
            Name = productIngr.Name,
            ProductId = productIngr.ProductId,
            AvailableProducts = GetProductIngrSelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ProdIngredViewModel model)
    {
         var existingProductIngredient = _userStore.GetProducIngredById(model.Id);
         if (existingProductIngredient is null)
         {
             return NotFound();
         }

          if (_userStore.ProductIngredNameExists(model.Name, model.ProductId, model.Id))
          {
              ModelState.AddModelError(nameof(model.Name), "That product already exists in the selected category.");
          }

         _userStore.UpdateProductIngred(new AppProductIngredience
         {
              Id = model.Id,
              ProdIngredienceName = model.ProdIngredienceName.Trim(),
              ProductId = existingProductIngredient.ProductId,
              Name = model.Name.Trim()
         });

        return RedirectToAction(nameof(Index));
    }

     public IActionResult Delete(int id)
    {
         var existingProductIngredient = _userStore.GetProducIngredById(id);
         if (existingProductIngredient is null)
         {
             return NotFound();
         }

        return View(existingProductIngredient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var existingProductIngredient = _userStore.GetProducIngredById(id);
         if (existingProductIngredient is null)
         {
             return NotFound();
         }

        _userStore.DeleteProductIngredients(id);
        return RedirectToAction(nameof(Index));
    }

     private IReadOnlyCollection<SelectListItem> GetProductIngrSelectList()
    {
        var productsWithIngredients = _userStore.GetAllProductIngredience()
            .Where(item => !string.IsNullOrWhiteSpace(item.ProdIngredienceName))
            .Select(item => item.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _userStore.GetAllProducts()
            .Where(product => !productsWithIngredients.Contains(product.Name))
            .Select(product => new SelectListItem(product.Name, product.Id.ToString()))
            .ToList();
    }
}
