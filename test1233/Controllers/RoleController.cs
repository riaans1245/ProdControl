using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

//[Authorize(Roles = "Admin")]
public class RoleController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
        var roles = _userStore.GetAllRoles();
        var userCounts = roles.ToDictionary(role => role.Id, role => _userStore.GetUserCountForRole(role.Id));

        return View(new RoleIndexViewModel
        {
            Roles = roles,
            UserCountsByRoleId = userCounts
        });
    }

    public IActionResult Create()
    {
        return View(new RoleFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RoleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_userStore.RoleNameExists(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "That role already exists.");
            return View(model);
        }

        _userStore.CreateRole(new AppRole { Name = model.Name.Trim(), IdentityCode = model.IdentityCode });
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var role = _userStore.GetRoleById(id);
        if (role is null)
        {
            return NotFound();
        }

        return View(new RoleFormViewModel
        {
            Id = role.Id,
            Name = role.Name,
            IdentityCode = role.IdentityCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(RoleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingRole = _userStore.GetRoleById(model.Id);
        if (existingRole is null)
        {
            return NotFound();
        }

        if (_userStore.RoleNameExists(model.Name, model.Id))
        {
            ModelState.AddModelError(nameof(model.Name), "That role already exists.");
            return View(model);
        }

        _userStore.UpdateRole(new AppRole
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            IdentityCode = model.IdentityCode
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var role = _userStore.GetRoleById(id);
        if (role is null)
        {
            return NotFound();
        }

        ViewData["UserCount"] = _userStore.GetUserCountForRole(id);
        return View(role);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var role = _userStore.GetRoleById(id);
        if (role is null)
        {
            return NotFound();
        }

        if (_userStore.RoleHasUsers(id))
        {
            ModelState.AddModelError(string.Empty, "This role cannot be deleted while users are assigned to it.");
            ViewData["UserCount"] = _userStore.GetUserCountForRole(id);
            return View(role);
        }

        _userStore.DeleteRole(id);
        return RedirectToAction(nameof(Index));
    }
}
