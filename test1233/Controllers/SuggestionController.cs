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

    public IActionResult Create()
    {
         return View(new SuggestionFormViewModel
         {
             MyName = string.Empty,
             Suggestion = string.Empty
         });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SuggestionFormViewModel model)
    {
         if (!ModelState.IsValid)
         {
             return View(model);
         }

         _userStore.SuggestionCreate(new AppSuggestion
         {
            MyName = model.MyName.Trim(),
            Suggestion = model.Suggestion.Trim()
         });

         return RedirectToAction(nameof(Index));
    }
}
