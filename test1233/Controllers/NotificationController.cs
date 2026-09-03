using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class NotificationController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, string sortOrder = "", int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NotificationSort"] = sortOrder == "notification_asc" ? "notification_desc" : "notification_asc";
        ViewData["UserSort"] = sortOrder == "user_asc" ? "user_desc" : "user_asc";

        var notifications = from notification in _userStore.GetAllNotifications()
                            select notification;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            notifications = notifications.Where(notification =>
                notification.UserName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        notifications = sortOrder switch
        {
            "notification_asc" => notifications
                .OrderBy(notification => notification.Notification)
                .ThenBy(notification => notification.UserName)
                .ThenBy(notification => notification.NotificationId),
            "notification_desc" => notifications
                .OrderByDescending(notification => notification.Notification)
                .ThenBy(notification => notification.UserName)
                .ThenBy(notification => notification.NotificationId),
            "user_asc" => notifications
                .OrderBy(notification => notification.UserName)
                .ThenBy(notification => notification.Notification)
                .ThenBy(notification => notification.NotificationId),
            "user_desc" => notifications
                .OrderByDescending(notification => notification.UserName)
                .ThenBy(notification => notification.Notification)
                .ThenBy(notification => notification.NotificationId),
            _ => notifications
                .OrderBy(notification => notification.NotificationId)
        };

        var filteredNotifications = notifications.ToList();
        var totalItems = filteredNotifications.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredNotifications
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppNotification>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    public IActionResult Create()
    {
        return View(new NotifiFormViewModel
        {
            AvailableUsers = GetUserSelectList()
        });
    }

     [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(NotifiFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            return View(model);
        }

        if (model.AllUsers)
        {
            var users = _userStore.GetAllUsers();
            if (users.Count == 0)
            {
                ModelState.AddModelError(nameof(model.AllUsers), "There are no users available to notify.");
                model.AvailableUsers = GetUserSelectList();
                return View(model);
            }

            foreach (var appUser in users)
            {
                _userStore.CreateNotification(new AppNotification
                {
                    Notification = model.Notification.Trim(),
                    UserId = appUser.Id,
                    UserName = appUser.Username
                });
            }

            return RedirectToAction(nameof(Index));
        }

        var user = _userStore.GetUserById(model.UserId);
         if (user is null)
         {
             ModelState.AddModelError(nameof(model.UserId), "Please choose a valid user.");
         }

         if (!ModelState.IsValid)
         {
             model.AvailableUsers = GetUserSelectList();
             return View(model);
        }

        _userStore.CreateNotification(new AppNotification
        {


            Notification = model.Notification.Trim(),
              UserId = user!.Id,
              UserName = user.Username
         });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var notifi = _userStore.GetNotificationById(id);
        if (notifi is null)
        {
            return NotFound();
        }

         return View(new NotifiFormViewModel
         {
            NotificationId = notifi.NotificationId,
            Notification = notifi.Notification,
            UserId = notifi.UserId,
            AvailableUsers = GetUserSelectList()
         });
    }

     [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(NotifiFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            return View(model);
        }

        var existingNotifi = _userStore.GetNotificationById(model.NotificationId);
        if (existingNotifi is null)
        {
            return NotFound();
        }

        var user = _userStore.GetUserById(model.UserId);
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.UserId), "Please choose a valid user.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            return View(model);
        }

        _userStore.UpdateNotification(new AppNotification
        {
            NotificationId = model.NotificationId,
            Notification = model.Notification.Trim(),
            UserId = user!.Id,
            UserName = user.Username
        });

        return RedirectToAction(nameof(Index));
    }

     public IActionResult Delete(int id)
    {
        var notifi = _userStore.GetDelNotificationById(id);
        if (notifi is null)
        {
            return NotFound();
        }

        return View(notifi);
    }

     [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int notificationId)
    {
        var notifi = _userStore.GetDelNotificationById(notificationId);
        if (notifi is null)
        {
            return NotFound(notifi);
        }

        _userStore.DeleteNotification(notificationId);
        return RedirectToAction(nameof(Index));
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance

    private IReadOnlyCollection<SelectListItem> GetUserSelectList()
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

    {
        return _userStore.GetAllUsers()
            .Select(user => new SelectListItem(user.Username, user.Id.ToString()))
            .ToList();
    }
}
