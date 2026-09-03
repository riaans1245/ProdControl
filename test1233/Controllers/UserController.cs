using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

//[Authorize(Roles = "Admin")]
public class UserController(IUserStore userStore, IWebHostEnvironment webHostEnvironment) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, string sortOrder = "", int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["SurnameSort"] = sortOrder == "surname_asc" ? "surname_desc" : "surname_asc";
        ViewData["EmailSort"] = sortOrder == "email_asc" ? "email_desc" : "email_asc";
        ViewData["CellSort"] = sortOrder == "cell_asc" ? "cell_desc" : "cell_asc";

        var users = from u in _userStore.GetAllUsers()
                    select u;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            users = users.Where(u =>
                u.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                u.Surname.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                u.EmailAddress.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        users = sortOrder switch
        {
            "surname_asc" => users
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "surname_desc" => users
                .OrderByDescending(u => u.Surname)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "email_asc" => users
                .OrderBy(u => u.EmailAddress)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "email_desc" => users
                .OrderByDescending(u => u.EmailAddress)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "cell_asc" => users
                .OrderBy(u => u.CellNo)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            "cell_desc" => users
                .OrderByDescending(u => u.CellNo)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id),
            _ => users
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.Id)
        };

        var filteredUsers = users.ToList();
        var totalItems = filteredUsers.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredUsers
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppUser>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    public IActionResult Create()
    {
        return View(new UserCreateViewModel
        {
            RoleId = 2,
            AvailableRoles = GetRoleSelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetRoleSelectList();
            return View(model);
        }

        if (_userStore.UsernameExists(model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "That username is already registered.");
        }

        if (_userStore.EmailAddressExists(model.EmailAddress))
        {
            ModelState.AddModelError(nameof(model.EmailAddress), "That email address is already registered.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetRoleSelectList();
            return View(model);
        }

        var selectedRole = _userStore.GetRoleById(model.RoleId);
        if (selectedRole is null)
        {
            ModelState.AddModelError(nameof(model.RoleId), "Please choose a valid role.");
            model.AvailableRoles = GetRoleSelectList();
            return View(model);
        }

        _userStore.CreateUser(new AppUser
        {
            Username = model.Username.Trim(),
            Name = model.Name.Trim(),
            Surname = model.Surname.Trim(),
            EmailAddress = model.EmailAddress.Trim(),
            CellNo = model.CellNo.Trim(),
            Password = model.Password,
            ProfileImagePath = SaveProfileImage(model.ProfileImage),
            RoleId = selectedRole.Id,
            Role = selectedRole.Name
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var user = _userStore.GetUserById(id);
        if (user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(BuildUserEditViewModel(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserEditViewModel model)
    {
        var existingUser = _userStore.GetUserById(model.Id);
        if (existingUser is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetRoleSelectList();
            model.CurrentProfileImagePath = existingUser.ProfileImagePath;
            return View(model);
        }

        if (_userStore.UsernameExists(model.Username, model.Id))
        {
            ModelState.AddModelError(nameof(model.Username), "That username is already registered.");
        }

        if (_userStore.EmailAddressExists(model.EmailAddress, model.Id))
        {
            ModelState.AddModelError(nameof(model.EmailAddress), "That email address is already registered.");
        }

        var selectedRole = _userStore.GetRoleById(model.RoleId);
        if (selectedRole is null)
        {
            ModelState.AddModelError(nameof(model.RoleId), "Please choose a valid role.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetRoleSelectList();
            model.CurrentProfileImagePath = existingUser.ProfileImagePath;
            return View(model);
        }

        var updatedProfileImagePath = existingUser.ProfileImagePath;

        if (model.RemoveProfileImage)
        {
            DeleteProfileImage(updatedProfileImagePath);
            updatedProfileImagePath = null;
        }

        if (model.ProfileImage is not null && model.ProfileImage.Length > 0)
        {
            DeleteProfileImage(updatedProfileImagePath);
            updatedProfileImagePath = SaveProfileImage(model.ProfileImage);
        }

        _userStore.UpdateUser(new AppUser
        {
            Id = existingUser.Id,
            Username = model.Username.Trim(),
            Name = model.Name.Trim(),
            Surname = model.Surname.Trim(),
            EmailAddress = model.EmailAddress.Trim(),
            CellNo = model.CellNo.Trim(),
            Password = string.IsNullOrWhiteSpace(model.Password) ? existingUser.Password : model.Password,
            ProfileImagePath = updatedProfileImagePath,
            RoleId = selectedRole!.Id,
            Role = selectedRole.Name
        });

        //TempData["UserEditMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Edit), new { id = existingUser.Id });
    }
    // bool ContactUs(ContactUs user);

    public IActionResult ContactUs()
    {
        return View(ContactUs());
    }


    public IActionResult Delete(int id)
    {
        var user = _userStore.GetUserById(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var user = _userStore.GetUserById(id);
        if (user is null)
        {
            return NotFound();
        }

        _userStore.DeleteUser(id);
        return RedirectToAction(nameof(Index));
    }

    private UserEditViewModel BuildUserEditViewModel(AppUser user)
    {
        return new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            Surname = user.Surname,
            EmailAddress = user.EmailAddress,
            CellNo = user.CellNo,
            RoleId = user.RoleId,
            CurrentProfileImagePath = user.ProfileImagePath,
            AvailableRoles = GetRoleSelectList()
        };
    }

    private IReadOnlyCollection<SelectListItem> GetRoleSelectList()
    {
        return _userStore.GetAllRoles()
            .Select(role => new SelectListItem(role.Name, role.Id.ToString()))
            .ToList();
    }

    private string? SaveProfileImage(IFormFile? profileImage)
    {
        if (profileImage is null || profileImage.Length == 0)
        {
            return null;
        }

        var uploadsDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "images", "user-profiles");
        Directory.CreateDirectory(uploadsDirectory);

        var fileExtension = Path.GetExtension(profileImage.FileName);
        var safeExtension = string.IsNullOrWhiteSpace(fileExtension) ? ".jpg" : fileExtension;
        var fileName = $"{Guid.NewGuid():N}{safeExtension}";
        var filePath = Path.Combine(uploadsDirectory, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        profileImage.CopyTo(stream);

        return $"/images/user-profiles/{fileName}";
    }

    private void DeleteProfileImage(string? profileImagePath)
    {
        if (string.IsNullOrWhiteSpace(profileImagePath) || !profileImagePath.StartsWith("/images/user-profiles/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relativePath = profileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
