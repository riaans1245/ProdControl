using test1233.Models;
using test1233.Services;
using System.Reflection;

namespace test1233.Tests;

public class InMemoryUserStoreTests
{
    [Fact]
    public void GetAllProducts_ReturnsSeededProductsWithExpectedCategories()
    {
        var store = new InMemoryUserStore();

        var products = store.GetAllProducts();

        Assert.Contains(products, product => product.Name == "Toast" && product.CategoryName == "Sides" && product.Price == 1.00m);
        Assert.Contains(products, product => product.Name == "EggBenedict" && product.CategoryName == "Breakfast" && product.Price == 19.50m);
        Assert.Contains(products, product => product.Name == "BuffaloWings" && product.CategoryName == "Specials" && product.Price == 18.00m);
    }

    [Fact]
    public void CreateUser_AssignsNextId_AndDefaultsCanBeManaged()
    {
        var store = new InMemoryUserStore();

        store.CreateUser(new AppUser
        {
            Username = "debug-user",
            Name = "Debug",
            Surname = "Tester",
            EmailAddress = "debug@example.com",
            CellNo = "0123456789",
            Password = "r!",
            RoleId = 2,
            Role = "Users"
        });

        var createdUser = store.GetAllUsers().Single(user => user.Username == "debug-user");

        Assert.True(createdUser.Id > 0);
        Assert.Equal("Users", createdUser.Role);
    }

    [Fact]
    public void UpdateCategory_ChangesStoredCategoryName()
    {
        var store = new InMemoryUserStore();
        var category = store.GetAllCategories().Single(item => item.Name == "Mains");

        var updated = store.UpdateCategory(new AppCategory
        {
            Id = category.Id,
            Name = "Main Meals"
        });

        var savedCategory = store.GetCategoryById(category.Id);

        Assert.True(updated);
        Assert.NotNull(savedCategory);
        Assert.Equal("Main Meals", savedCategory!.Name);
    }

    [Fact]
    public void CreateProduct_RoundsPriceToTwoDecimalPlaces()
    {
        var store = new InMemoryUserStore();
        var category = store.GetAllCategories().First();

        store.CreateProduct(CreateProduct("Rounded Product", 12.345m, category.Id, category.Name));

        var savedProduct = store.GetAllProducts().Single(product => product.Name == "Rounded Product");

        Assert.Equal(12.35m, savedProduct.Price);
    }

    [Fact]
    public void UpdateProduct_RoundsPriceToTwoDecimalPlaces()
    {
        var store = new InMemoryUserStore();
        var product = store.GetAllProducts().First();

        var updated = store.UpdateProduct(CreateProduct(product.Name, 7.995m, product.CategoryId, product.CategoryName, product.Id));

        var savedProduct = store.GetProductById(product.Id);

        Assert.True(updated);
        Assert.NotNull(savedProduct);
        Assert.Equal(8.00m, savedProduct!.Price);
    }

    private static AppProduct CreateProduct(string name, decimal price, int categoryId, string categoryName, int id = 0)
    {
        var product = new AppProduct
        {
            Id = id,
            Name = name,
            CategoryId = categoryId,
            CategoryName = categoryName
        };

        typeof(AppProduct)
            .GetProperty(nameof(AppProduct.Price), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(product, price);

        return product;
    }
}
