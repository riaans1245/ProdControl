using test1233.Models;

namespace test1233.Services;

public class InMemoryUserStore : IUserStore
{
    private readonly Dictionary<string, string> _magicLinks = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<AppProduct> _products =
    [
        new AppProduct { Id = 1, Name = "Toast", Price = 1.00m, CategoryId = 4, CategoryName = "Sides" },
        new AppProduct { Id = 2, Name = "Chips", Price = 3.00m, CategoryId = 4, CategoryName = "Sides" },
        new AppProduct { Id = 3, Name = "Tomato", Price = 0.50m, CategoryId = 4, CategoryName = "Sides" },
        new AppProduct { Id = 4, Name = "Bacon", Price = 2.50m, CategoryId = 4, CategoryName = "Sides" },
        new AppProduct { Id = 5, Name = "Egg", Price = 1.00m, CategoryId = 4, CategoryName = "Sides" },
        new AppProduct { Id = 6, Name = "ScrambledEggonToast", Price = 13.25m, CategoryId = 1, CategoryName = "Breakfast" },
        new AppProduct { Id = 7, Name = "EnglisgBreakFast", Price = 29.00m, CategoryId = 1, CategoryName = "Breakfast" },
        new AppProduct { Id = 8, Name = "EggBenedict", Price = 19.50m, CategoryId = 1, CategoryName = "Breakfast" },
        new AppProduct { Id = 9, Name = "AvoOnToast", Price = 16.50m, CategoryId = 1, CategoryName = "Breakfast" },
        new AppProduct { Id = 10, Name = "SteakEggChips", Price = 35.00m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 11, Name = "T-BoneandChips", Price = 32.00m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 12, Name = "PorkBelly", Price = 29.99m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 13, Name = "ChickenBurger", Price = 23.00m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 14, Name = "BeefBurger", Price = 25.00m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 15, Name = "Lisagne", Price = 22.50m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 16, Name = "LambChops", Price = 27.50m, CategoryId = 2, CategoryName = "Mains" },
        new AppProduct { Id = 17, Name = "FlatWhite", Price = 3.50m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 18, Name = "Cuppochine", Price = 4.50m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 19, Name = "Americano", Price = 4.50m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 20, Name = "ChiaLatte", Price = 5.00m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 21, Name = "Coke", Price = 3.50m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 22, Name = "Sprite", Price = 3.50m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 23, Name = "OrangeJuice", Price = 3.00m, CategoryId = 5, CategoryName = "Drinks" },
        new AppProduct { Id = 24, Name = "FishandChips", Price = 22.00m, CategoryId = 3, CategoryName = "Specials" },
        new AppProduct { Id = 25, Name = "BuffaloWings", Price = 18.00m, CategoryId = 3, CategoryName = "Specials" },
    ];

    private readonly List<AppCategory> _categories =
    [
        new AppCategory { Id = 1, Name = "Breakfast" },
        new AppCategory { Id = 2, Name = "Mains" },
        new AppCategory { Id = 3, Name = "Specials" },
        new AppCategory { Id = 4, Name = "Sides" },
        new AppCategory { Id = 5, Name = "Drinks" }
    ];

    private readonly List<AppRole> _roles =
    [
        new AppRole { Id = 1, Name = "Admin", IdentityCode = 1 },
        new AppRole { Id = 2, Name = "Users", IdentityCode = 2 }
    ];

      private readonly List<ContactUs> _contactUs =
    [

    ];

    private readonly List<AppUser> _users =
    [
        new AppUser
        {
            Id = 1,
            Username = "a",
            Name = "System",
            Surname = "Administrator",
            EmailAddress = "riaan.strydom@gmail.com",
            CellNo = "0000000000",
            Password = "a",
            RoleId = 1,
            Role = "Admin"
        }
    ];

    private readonly object _lock = new();

    public AppUser? ValidateUser(string username, string password)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(user =>
                string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase) &&
                user.Password == password);
        }
    }

    public bool UsernameExists(string username)
    {
        return UsernameExists(username, null);
    }

    public bool UsernameExists(string username, int? excludeUserId)
    {
        lock (_lock)
        {
            return _users.Any(user =>
                user.Id != excludeUserId &&
                string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool EmailAddressExists(string emailAddress)
    {
        return EmailAddressExists(emailAddress, null);
    }

    public bool EmailAddressExists(string emailAddress, int? excludeUserId)
    {
        lock (_lock)
        {
            return _users.Any(user =>
                user.Id != excludeUserId &&
                string.Equals(user.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool RoleNameExists(string roleName, int? excludeRoleId = null)
    {
        lock (_lock)
        {
            return _roles.Any(role =>
                role.Id != excludeRoleId &&
                string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public void CreateUser(AppUser user)
    {
        lock (_lock)
        {
            user.Id = _users.Count == 0 ? 1 : _users.Max(item => item.Id) + 1;
            _users.Add(user);
        }
    }

    public IReadOnlyCollection<AppUser> GetAllUsers()
    {
        lock (_lock)
        {
            return _users.ToList().AsReadOnly();
        }
    }

    public AppUser? GetUserById(int id)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(user => user.Id == id);
        }
    }

    public AppUser? GetUserByEmailAddress(string emailAddress)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(user =>
                string.Equals(user.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool UpdateUser(AppUser user)
    {
        lock (_lock)
        {
            var existingUser = _users.FirstOrDefault(item => item.Id == user.Id);
            if (existingUser is null)
            {
                return false;
            }

            existingUser.Username = user.Username;
            existingUser.Name = user.Name;
            existingUser.Surname = user.Surname;
            existingUser.EmailAddress = user.EmailAddress;
            existingUser.CellNo = user.CellNo;
            existingUser.Password = user.Password;
            existingUser.RoleId = user.RoleId;
            existingUser.Role = user.Role;

            return true;
        }
    }

    public bool ContactUs(ContactUs user)
     {
         lock (_lock)
         {
            var nextId = _contactUs.Count == 0 ? 1 : _contactUs.Max(item => item.Id) + 1;
            _contactUs.Add(new ContactUs
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                CellNo = user.CellNo,
                EmailAddress = user.EmailAddress
            });
        }

        return false;
    }


    public bool DeleteUser(int id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(item => item.Id == id);
            if (user is null)
            {
                return false;
            }

            _users.Remove(user);
            return true;
        }
    }

    public string CreateMagicLink(string emailAddress)
    {
        lock (_lock)
        {
            var token = Guid.NewGuid().ToString("N");
            _magicLinks[token] = emailAddress;
            return token;
        }
    }

    public AppUser? ConsumeMagicLink(string token)
    {
        lock (_lock)
        {
            if (!_magicLinks.TryGetValue(token, out var emailAddress))
            {
                return null;
            }

            _magicLinks.Remove(token);

            return _users.FirstOrDefault(user =>
                string.Equals(user.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool CategoryNameExists(string categoryName, int? excludeCategoryId = null)
    {
        lock (_lock)
        {
            return _categories.Any(category =>
                category.Id != excludeCategoryId &&
                string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyCollection<AppCategory> GetAllCategories()
    {
        lock (_lock)
        {
            return _categories
                .OrderBy(category => category.Id)
                .Select(category => new AppCategory
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToList()
                .AsReadOnly();
        }
    }

    public AppCategory? GetCategoryById(int id)
    {
        lock (_lock)
        {
            var category = _categories.FirstOrDefault(item => item.Id == id);
            return category is null
                ? null
                : new AppCategory
                {
                    Id = category.Id,
                    Name = category.Name
                };
        }
    }

    public void CreateCategory(AppCategory category)
    {
        lock (_lock)
        {
            var nextId = _categories.Count == 0 ? 1 : _categories.Max(item => item.Id) + 1;
            _categories.Add(new AppCategory
            {
                Id = nextId,
                Name = category.Name
            });
        }
    }

    public bool UpdateCategory(AppCategory category)
    {
        lock (_lock)
        {
            var existingCategory = _categories.FirstOrDefault(item => item.Id == category.Id);
            if (existingCategory is null)
            {
                return false;
            }

            existingCategory.Name = category.Name;
            return true;
        }
    }

    public bool DeleteCategory(int id)
    {
        lock (_lock)
        {
            if (_products.Any(product => product.CategoryId == id))
            {
                return false;
            }

            var category = _categories.FirstOrDefault(item => item.Id == id);
            if (category is null)
            {
                return false;
            }

            _categories.Remove(category);
            return true;
        }
    }

    public bool ProductNameExists(string productName, int categoryId, int? excludeProductId = null)
    {
        lock (_lock)
        {
            return _products.Any(product =>
                product.Id != excludeProductId &&
                product.CategoryId == categoryId &&
                string.Equals(product.Name, productName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyCollection<AppProduct> GetAllProducts()
    {
        lock (_lock)
        {
           return _products
            //.Where(product => product.CategoryId != 4)
            .OrderBy(product => product.Id)
            .Select(product => new AppProduct 
            { 
                Id = product.Id, 
                Name = product.Name, 
                Price = product.Price, 
                CategoryId = product.CategoryId, 
                CategoryName = product.CategoryName 
            })
            .ToList()
            .AsReadOnly();
        }
    }

    public AppProduct? GetProductById(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(item => item.Id == id);
            return product is null
                ? null
                : new AppProduct
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    CategoryName = product.CategoryName
                };
        }
    }

    public void CreateProduct(AppProduct product)
    {
        lock (_lock)
        {
            var nextId = _products.Count == 0 ? 1 : _products.Max(item => item.Id) + 1;
            _products.Add(new AppProduct
            {
                Id = nextId,
                Name = product.Name,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.CategoryName
            });
        }
    }

    public bool UpdateProduct(AppProduct product)
    {
        lock (_lock)
        {
            var existingProduct = _products.FirstOrDefault(item => item.Id == product.Id);
            if (existingProduct is null)
            {
                return false;
            }

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.CategoryName = product.CategoryName;
            return true;
        }
    }

    public bool DeleteProduct(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(item => item.Id == id);
            if (product is null)
            {
                return false;
            }

            _products.Remove(product);
            return true;
        }
    }

    public IReadOnlyCollection<AppRole> GetAllRoles()
    {
        lock (_lock)
        {
            return _roles
                .OrderBy(role => role.Id)
                .Select(role => new AppRole { Id = role.Id, Name = role.Name, IdentityCode = role.IdentityCode })
                .ToList()
                .AsReadOnly();
        }
    }

    public AppRole? GetRoleById(int id)
    {
        lock (_lock)
        {
            var role = _roles.FirstOrDefault(item => item.Id == id);
            return role is null ? null : new AppRole { Id = role.Id, Name = role.Name, IdentityCode = role.IdentityCode };
        }
    }

    public void CreateRole(AppRole role)
    {
        lock (_lock)
        {
            var nextId = _roles.Count == 0 ? 1 : _roles.Max(item => item.Id) + 1;
            _roles.Add(new AppRole
            {
                Id = nextId,
                Name = role.Name,
                IdentityCode = role.IdentityCode
            });
        }
    }

    public bool UpdateRole(AppRole role)
    {
        lock (_lock)
        {
            var existingRole = _roles.FirstOrDefault(item => item.Id == role.Id);
            if (existingRole is null)
            {
                return false;
            }

            var previousName = existingRole.Name;
            existingRole.Name = role.Name;
            existingRole.IdentityCode = role.IdentityCode;

            foreach (var user in _users.Where(user => user.RoleId == role.Id))
            {
                user.Role = role.Name;
            }

            return !string.Equals(previousName, role.Name, StringComparison.Ordinal);
        }
    }

    public bool DeleteRole(int id)
    {
        lock (_lock)
        {
            if (_users.Any(user => user.RoleId == id))
            {
                return false;
            }

            var role = _roles.FirstOrDefault(item => item.Id == id);
            if (role is null)
            {
                return false;
            }

            _roles.Remove(role);
            return true;
        }
    }

    public bool RoleHasUsers(int id)
    {
        lock (_lock)
        {
            return _users.Any(user => user.RoleId == id);
        }
    }

    public int GetUserCountForRole(int id)
    {
        lock (_lock)
        {
            return _users.Count(user => user.RoleId == id);
        }
    }

    public IReadOnlyCollection<ContactUs> GetAllContactUs()
    {
        lock (_lock)
        {
            return _contactUs
                .OrderBy(contactus => contactus.Id)
                .Select(contactus => new ContactUs
                {
                    Id = contactus.Id,
                    Name = contactus.Name,
                    Surname = contactus.Surname,
                    CellNo = contactus.CellNo,
                    EmailAddress = contactus.EmailAddress
                })
                .ToList()
                .AsReadOnly();

        }
    }

}
