using test1233.Models;

namespace test1233.Services;

public interface IUserStore
{
    AppUser? ValidateUser(string username, string password);

    bool UsernameExists(string username);

    bool UsernameExists(string username, int? excludeUserId);

    bool EmailAddressExists(string emailAddress);

    bool EmailAddressExists(string emailAddress, int? excludeUserId);

    bool RoleNameExists(string roleName, int? excludeRoleId = null);

    void CreateUser(AppUser user);

    IReadOnlyCollection<AppUser> GetAllUsers();

    IReadOnlyCollection<AppUser> GetAllUsersList();

    IReadOnlyCollection<AppTokens> GetAllTokens();

    void CreateToken(AppTokens tokens);

    AppUser? GetUserById(int id);

    AppUser? GetUserByEmailAddress(string emailAddress);

    bool UpdateUser(AppUser user);

    bool ContactUs(ContactUs user);

    bool DeleteUser(int id);

    string CreateMagicLink(string emailAddress);

    AppUser? ConsumeMagicLink(string token);

    bool CategoryNameExists(string categoryName, int? excludeCategoryId = null);

    IReadOnlyCollection<AppCategory> GetAllCategories();

    IReadOnlyCollection<ContactUs> GetAllContactUs();

    AppCategory? GetCategoryById(int id);

    //AppTokens? GetTokensById(int id);

    void CreateCategory(AppCategory category);

    bool UpdateCategory(AppCategory category);

    bool DeleteCategory(int id);

    bool ProductNameExists(string productName, int categoryId, int? excludeProductId = null);

    bool TokenNameExists(string tokenName, int userId, int? excludeTokenId = null);

    IReadOnlyCollection<AppProduct> GetAllProducts();

    AppProduct? GetProductById(int id);

    void CreateProduct(AppProduct product);

    bool UpdateProduct(AppProduct product);

    bool DeleteProduct(int id);

    IReadOnlyCollection<AppRole> GetAllRoles();

    AppRole? GetRoleById(int id);

    void CreateRole(AppRole role);

    bool UpdateRole(AppRole role);

    bool DeleteRole(int id);

    bool RoleHasUsers(int id);

    int GetUserCountForRole(int id);
}
