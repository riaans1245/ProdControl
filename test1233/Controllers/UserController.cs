using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

//[Authorize(Roles = "Admin")]
public class UserController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;

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
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = GetRoleSelectList();
            return View(model);
        }

        var existingUser = _userStore.GetUserById(model.Id);
        if (existingUser is null)
        {
            return NotFound();
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
            return View(model);
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
            RoleId = selectedRole!.Id,
            Role = selectedRole.Name
        });

        return RedirectToAction(nameof(Index));
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
            AvailableRoles = GetRoleSelectList()
        };
    }

    private IReadOnlyCollection<SelectListItem> GetRoleSelectList()
    {
        return _userStore.GetAllRoles()
            .Select(role => new SelectListItem(role.Name, role.Id.ToString()))
            .ToList();
    }
}
