using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class TableController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(int page = 1)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var tables = _userStore.GetAllTables().ToList();
        var totalItems = tables.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = tables
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppTables>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    public IActionResult Create()
    {
         return View(new TableFormViewModel
         {
             TableName = string.Empty,
             TableNumber = 0
         });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TableFormViewModel model)
    {
         if (!ModelState.IsValid)
         {
             return View(model);
         }

         _userStore.TableCreate(new AppTables
         {
            TableName = model.TableName,
            TableNumber = model.TableNumber
         });

         return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int TableId)
    {
        var tables = _userStore.GetTablesById(TableId);
        if (tables is null)
        {
            return NotFound();
        }

        return View(new TableFormViewModel
        {
            TableId = tables.TableId,
            TableName = tables.TableName,
            TableNumber = tables.TableNumber
        });
    }


    [HttpPost]
     [ValidateAntiForgeryToken]
     public IActionResult Edit(TableFormViewModel model)
     {
         if (!ModelState.IsValid)
         {
             return View(model);
         }

    //     var existingRole = _userStore.GetRoleById(model.Id);
    //     if (existingRole is null)
    //     {
    //         return NotFound();
    //     }

    //     if (_userStore.RoleNameExists(model.Name, model.Id))
    //     {
    //         ModelState.AddModelError(nameof(model.Name), "That role already exists.");
    //         return View(model);
    //     }

    //     _userStore.UpdateRole(new AppRole
    //     {
    //         Id = model.Id,
    //         Name = model.Name.Trim(),
    //         IdentityCode = model.IdentityCode
    //     });

         return RedirectToAction(nameof(Index));
     }

    public IActionResult Delete(int id)
     {
         var tables = _userStore.GetTablesById(id);
         if (tables is null)
         {
             return NotFound();
         }
         return View(tables);
    }

    [HttpPost, ActionName("Delete")]
     [ValidateAntiForgeryToken]
     public IActionResult DeleteConfirmed(int TableId)
     {
         var tables = _userStore.GetTablesById(TableId);
         if (tables is null)
         {
             return NotFound();
         }
         _userStore.DeleteTable(TableId);
         return RedirectToAction(nameof(Index));
     }

    private AppUser? GetCurrentUser()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return _userStore.GetAllUsers()
            .FirstOrDefault(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
    }
}
