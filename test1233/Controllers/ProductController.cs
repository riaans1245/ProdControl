using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic.FileIO;
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
            .Prepend(new SelectListItem("-- Please Select--", "0"))
            .ToList();
    }

    public IActionResult BulkUpload()
    {
        return View(new ProdBulkUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BulkUpload(ProdBulkUploadViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var csvFile = model.CsvFile!;
        if (!string.Equals(Path.GetExtension(csvFile.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.CsvFile), "Please upload a valid CSV file.");
            return View(model);
        }

        var categories = _userStore.GetAllCategories().ToDictionary(category => category.Id);
        var productsToCreate = new List<AppProduct>();
        var importedProductKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skippedRowsCount = 0;

        try
        {
            using var parser = new TextFieldParser(csvFile.OpenReadStream())
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true,
                TrimWhiteSpace = true
            };
            parser.SetDelimiters(",");

            var headers = parser.EndOfData ? null : parser.ReadFields();
            if (!HasExpectedHeaders(headers))
            {
                ModelState.AddModelError(nameof(model.CsvFile), "The CSV header must be ProdName,ProdCost,CatId.");
                return View(model);
            }

            while (!parser.EndOfData)
            {
                string[]? columns;
                try
                {
                    columns = parser.ReadFields();
                }
                catch (MalformedLineException)
                {
                    skippedRowsCount++;
                    continue;
                }

                if (columns is null || columns.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                if (columns.Length != 3 ||
                    string.IsNullOrWhiteSpace(columns[0]) ||
                    !decimal.TryParse(columns[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ||
                    price < 0 ||
                    !int.TryParse(columns[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var categoryId) ||
                    !categories.TryGetValue(categoryId, out var category))
                {
                    skippedRowsCount++;
                    continue;
                }

                var name = columns[0].Trim();
                var productKey = $"{categoryId}\0{name}";
                if (_userStore.ProductNameExists(name, categoryId) || !importedProductKeys.Add(productKey))
                {
                    skippedRowsCount++;
                    continue;
                }

                productsToCreate.Add(new AppProduct
                {
                    Name = name,
                    Price = price,
                    CategoryId = categoryId,
                    CategoryName = category.Name
                });
            }
        }
        catch (IOException)
        {
            ModelState.AddModelError(nameof(model.CsvFile), "The CSV file could not be read. Please try again.");
            return View(model);
        }
        catch (MalformedLineException)
        {
            ModelState.AddModelError(nameof(model.CsvFile), "The CSV header is malformed.");
            return View(model);
        }

        foreach (var product in productsToCreate)
        {
            _userStore.CreateProduct(product);
        }

        if (productsToCreate.Count == 0)
        {
            TempData["ErrorMessage"] = "No valid new products were found in the uploaded file.";
        }
        else
        {
            var confirmation = $"Successfully imported {productsToCreate.Count} product(s).";
            TempData["SuccessMessage"] = skippedRowsCount == 0
                ? confirmation
                : $"{confirmation} {skippedRowsCount} row(s) were skipped because they were invalid or duplicated.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static bool HasExpectedHeaders(string[]? headers)
    {
        string[] expectedHeaders = ["ProdName", "ProdCost", "CatId"];
        return headers is not null &&
               headers.Length == expectedHeaders.Length &&
               headers.Zip(expectedHeaders).All(pair =>
                   string.Equals(pair.First.Trim(), pair.Second, StringComparison.OrdinalIgnoreCase));
    }

}
