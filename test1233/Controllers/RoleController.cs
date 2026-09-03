using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[Authorize(Roles = "Admin")]
public class RoleController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, string sortOrder = "", int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSort"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
        ViewData["CodeSort"] = sortOrder == "code_asc" ? "code_desc" : "code_asc";

        var roles = from role in _userStore.GetAllRoles()
                    select role;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            roles = roles.Where(role =>
                role.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        roles = sortOrder switch
        {
            "name_asc" => roles
                .OrderBy(role => role.Name)
                .ThenBy(role => role.IdentityCode)
                .ThenBy(role => role.Id),
            "name_desc" => roles
                .OrderByDescending(role => role.Name)
                .ThenBy(role => role.IdentityCode)
                .ThenBy(role => role.Id),
            "code_asc" => roles
                .OrderBy(role => role.IdentityCode)
                .ThenBy(role => role.Name)
                .ThenBy(role => role.Id),
            "code_desc" => roles
                .OrderByDescending(role => role.IdentityCode)
                .ThenBy(role => role.Name)
                .ThenBy(role => role.Id),
            _ => roles
                .OrderBy(role => role.Name)
                .ThenBy(role => role.Id)
        };

        var filteredRoles = roles.ToList();
        var totalItems = filteredRoles.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredRoles
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();
        var userCounts = items.ToDictionary(role => role.Id, role => _userStore.GetUserCountForRole(role.Id));

        return View(new RoleIndexViewModel
        {
            Roles = items,
            UserCountsByRoleId = userCounts,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
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

        _userStore.CreateRole(new AppRole
        {
            Name = model.Name.Trim(),
            IdentityCode = model.IdentityCode
        });
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
