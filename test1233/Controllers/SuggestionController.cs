using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;
public class SuggestionController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
         return View(_userStore.GetAllSuggestions());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public object SuggestionCreate(AppSuggestion model)
    {
         if (!ModelState.IsValid)
         {
             return View(model);
         }

         _userStore.SuggestionCreate(new AppSuggestion
         {
            MyName = model.MyName,
            Suggestion = model.Suggestion,
         });
         return RedirectToAction();
    }
}
