using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[Authorize(Roles = "Admin")]
public class UserController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
        return View(_userStore.GetAllUsers());
    }

    public IActionResult Create()
    {
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
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
            return View(model);
        }

        var defaultRole = _userStore.GetRoleById(2);
        if (defaultRole is null)
        {
            ModelState.AddModelError(string.Empty, "The default Users role could not be found.");
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
            RoleId = defaultRole.Id,
            Role = defaultRole.Name
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var user = _userStore.GetUserById(id);
        if (user is null)
        {
            return NotFound();
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
            .Select(role => new SelectListItem($"{role.Id} - {role.Name}", role.Id.ToString()))
            .ToList();
    }
}
