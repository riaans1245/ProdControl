using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class NotificationController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
        return View(_userStore.GetAllNotifications());
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

        var user = _userStore.GetUserById(model.UserId);
         if (user is null)
         {
             ModelState.AddModelError(nameof(model.UserId), "Please choose a valid category.");
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
            return NotFound();
        }

        _userStore.DeleteNotification(notificationId);
        return RedirectToAction(nameof(Index));
    }

      private IReadOnlyCollection<SelectListItem> GetUserSelectList()
    {
        return _userStore.GetAllUsers()
            .Select(user => new SelectListItem(user.Username, user.Id.ToString()))
            .ToList();
    }
}
