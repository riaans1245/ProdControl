using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class ContactUsController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult ContactUs()
    {
        return View(_userStore.GetAllContactUs());
    }

    public IActionResult Delete(int id)
    {
        var contact = _userStore.GetDelContactUsById(id);
        if (contact is null)
        {
            return NotFound();
        }

        return View(contact);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int Id)
    {
        var contact = _userStore.GetDelContactUsById(Id);
        if (contact is null)
        {
            return NotFound(contact);
        }

        _userStore.DeleteContactUs(Id);
        return RedirectToAction(nameof(ContactUs));
    }
}