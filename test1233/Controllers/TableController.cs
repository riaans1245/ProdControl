using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class TableController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string sortOrder = "", int page = 1)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["CurrentSort"] = sortOrder;
        ViewData["TableNameSort"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
        ViewData["TableNumberSort"] = sortOrder == "number_asc" ? "number_desc" : "number_asc";
        ViewData["UserSort"] = sortOrder == "user_asc" ? "user_desc" : "user_asc";
        ViewData["BookedForSort"] = sortOrder == "bookedfor_asc" ? "bookedfor_desc" : "bookedfor_asc";

        IEnumerable<AppTables> tables = _userStore.GetAllTables();
        tables = sortOrder switch
        {
            "name_asc" => tables
                .OrderBy(table => table.TableName)
                .ThenBy(table => table.TableNumber)
                .ThenBy(table => table.TableId),
            "name_desc" => tables
                .OrderByDescending(table => table.TableName)
                .ThenBy(table => table.TableNumber)
                .ThenBy(table => table.TableId),
            "number_asc" => tables
                .OrderBy(table => table.TableNumber)
                .ThenBy(table => table.TableName)
                .ThenBy(table => table.TableId),
            "number_desc" => tables
                .OrderByDescending(table => table.TableNumber)
                .ThenBy(table => table.TableName)
                .ThenBy(table => table.TableId),
            "user_asc" => tables
                .OrderBy(table => table.Username)
                .ThenBy(table => table.TableName)
                .ThenBy(table => table.TableId),
            "user_desc" => tables
                .OrderByDescending(table => table.Username)
                .ThenBy(table => table.TableName)
                .ThenBy(table => table.TableId),
            "bookedfor_asc" => tables
                .OrderBy(table => table.BookedForUtc ?? DateTime.MaxValue)
                .ThenBy(table => table.TableName)
                .ThenBy(table => table.TableId),
            "bookedfor_desc" => tables
                .OrderByDescending(table => table.BookedForUtc ?? DateTime.MinValue)
                .ThenBy(table => table.TableName)
                .ThenBy(table => table.TableId),
            _ => tables
                .OrderBy(table => table.TableId)
        };

        var tableList = tables.ToList();
        var totalItems = tableList.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = tableList
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
            TableNumber = model.TableNumber,
            UserId = 0,
            Username = string.Empty,
            BookedForUtc = null
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
            TableNumber = tables.TableNumber,
            UserId = tables.UserId,
            BookedForUtc = tables.BookedForUtc
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

         var tables = _userStore.GetTablesById(model.TableId);
         if (tables is null)
         {
             return NotFound();
         }

         if (_userStore.TableNameExists(model.TableName, model.TableId))
         {
             ModelState.AddModelError(nameof(model.TableName), "That table already exists.");
             return View(model);
         }

          _userStore.UpdateTable(new AppTables
          {
              TableId = model.TableId,
              TableName = model.TableName.Trim(),
              TableNumber = model.TableNumber,
              UserId = tables.UserId,
              Username = tables.Username,
              BookedForUtc = tables.BookedForUtc
          });

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
