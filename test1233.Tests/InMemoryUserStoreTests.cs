using test1233.Models;
using test1233.Services;

namespace test1233.Tests;

public class InMemoryUserStoreTests
{
    [Fact]
    public void GetAllProducts_ReturnsSeededProductsWithExpectedCategories()
    {
        var store = new InMemoryUserStore();

        var products = store.GetAllProducts();

        Assert.Contains(products, product => product.Name == "PC" && product.CategoryName == "InformationTechnology");
        Assert.Contains(products, product => product.Name == "Files" && product.CategoryName == "Finance");
        Assert.Contains(products, product => product.Name == "CNC Machines" && product.CategoryName == "Engineering");
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
        var category = store.GetAllCategories().Single(item => item.Name == "Finance");

        var updated = store.UpdateCategory(new AppCategory
        {
            Id = category.Id,
            Name = "FinanceOps"
        });

        var savedCategory = store.GetCategoryById(category.Id);

        Assert.True(updated);
        Assert.NotNull(savedCategory);
        Assert.Equal("FinanceOps", savedCategory!.Name);
    }
}
