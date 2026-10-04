using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using test1233.Controllers;
using test1233.Models;
using test1233.Services;

namespace test1233.Tests;

public class ProductControllerTests
{
    [Fact]
    public void BulkUpload_ImportsValidCsvRowsIncludingQuotedNames()
    {
        var store = new InMemoryUserStore();
        var category = store.GetAllCategories().First();
        var controller = CreateController(store);
        var model = CreateModel(
            $"ProdName,ProdCost,CatId\r\n\"Soup, large\",12.50,{category.Id}\r\nInvalid,abc,{category.Id}");

        var result = controller.BulkUpload(model);

        Assert.IsType<RedirectToActionResult>(result);
        var product = Assert.Single(store.GetAllProducts(), item => item.Name == "Soup, large");
        Assert.Equal(12.50m, product.Price);
        Assert.Equal(category.Id, product.CategoryId);
        Assert.Contains("1 row(s) were skipped", Assert.IsType<string>(controller.TempData["SuccessMessage"]));
    }

    [Fact]
    public void BulkUpload_SkipsExistingAndFileDuplicates()
    {
        var store = new InMemoryUserStore();
        var existingProduct = store.GetAllProducts().First();
        var category = store.GetAllCategories().First(item => item.Id == existingProduct.CategoryId);
        var controller = CreateController(store);
        var model = CreateModel(
            $"ProdName,ProdCost,CatId\r\n{existingProduct.Name},10,{category.Id}\r\nNew item,11,{category.Id}\r\nnew item,12,{category.Id}");

        controller.BulkUpload(model);

        Assert.Single(store.GetAllProducts(), item =>
            item.CategoryId == category.Id &&
            string.Equals(item.Name, "New item", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("2 row(s) were skipped", Assert.IsType<string>(controller.TempData["SuccessMessage"]));
    }

    [Fact]
    public void BulkUpload_RejectsUnexpectedHeaders()
    {
        var store = new InMemoryUserStore();
        var originalCount = store.GetAllProducts().Count;
        var controller = CreateController(store);
        var model = CreateModel("Name,Price,Category\r\nSoup,10,1");

        var result = controller.BulkUpload(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(originalCount, store.GetAllProducts().Count);
    }

    private static ProductController CreateController(InMemoryUserStore store)
    {
        var httpContext = new DefaultHttpContext();
        return new ProductController(store)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new TestTempDataProvider())
        };
    }

    private static ProdBulkUploadViewModel CreateModel(string csv)
    {
        var bytes = Encoding.UTF8.GetBytes(csv);
        return new ProdBulkUploadViewModel
        {
            CsvFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "CsvFile", "products.csv")
        };
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
