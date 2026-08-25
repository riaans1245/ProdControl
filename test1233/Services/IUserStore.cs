using Microsoft.AspNetCore.Mvc;
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

    bool TableNameExists(string TableName, int? excludeTableId = null);

    void CreateUser(AppUser user);

    void SuggestionCreate(AppSuggestion suggest);

    void TableCreate(AppTables tables);

    IReadOnlyCollection<AppUser> GetAllUsers();

    IReadOnlyCollection<AppUser> GetAllUsersList();

    IReadOnlyCollection<AppTokens> GetAllTokens();

    IReadOnlyCollection<AppCartItem> GetCartItemsForUser(int userId);

    IReadOnlyCollection<AppUsedToken> GetAllUsedTokens();

    IReadOnlyCollection<AppOrder> GetPendingOrdersForUser(int userId);

    AppReceipt? GetLatestReceiptForUser(int userId);

    IReadOnlyCollection<AppSuggestion> GetAllSuggestions();

    IReadOnlyCollection<AppNotification> GetAllNotifications();

    void CreateToken(AppTokens tokens);

    void AddOrUpdateCartItem(AppCartItem cartItem);

    bool RemoveCartItem(int userId, int productId);

    void ClearCart(int userId);

    AppOrder CreatePendingOrder(int userId, string username, IReadOnlyCollection<AppCartItem> items, DateTime placedAtUtc);

    AppReceipt? ConfirmPendingOrdersPayment(int userId, string username, DateTime paidAtUtc);

    void RecordUsedToken(AppUsedToken usedToken);

    void CreateNotification(AppNotification notification);

    bool UpdateToken(AppTokens tokens);

    bool UpdateNotification(AppNotification notification);

    IReadOnlyCollection<AppTables> GetAllTables();

    IReadOnlyCollection<AppTables> GetTablesGroupedByUser(string currentUsername);

    bool DeleteToken(int id);

    bool DeleteNotification(int id);

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

    IReadOnlyCollection<AppProduct> GetAllUserOrders();

    AppProduct? GetProductById(int id);

    AppTokens? GetTokenById(int id);

    AppNotification? GetDelNotificationById(int id);

    AppNotification? GetNotificationById(int id);

    void CreateProduct(AppProduct product);

    bool UpdateProduct(AppProduct product);

    bool DeleteProduct(int id);

    IReadOnlyCollection<AppRole> GetAllRoles();

    AppRole? GetRoleById(int id);

    //AppSuggestion? GetSuggestionById(int Id);

    AppSuggestion? GetSuggestById(int id);

    AppTables? GetTablesById(int id);

    bool DeleteSuggestion(int id);

    bool DeleteTable(int id);

    void CreateRole(AppRole role);

    bool UpdateRole(AppRole role);

    bool UpdateTable(AppTables tables);

    bool DeleteRole(int id);

    bool RoleHasUsers(int id);

    int GetUserCountForRole(int id);
}
