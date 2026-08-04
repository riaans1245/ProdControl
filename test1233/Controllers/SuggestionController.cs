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
            MyName = model.MyName,
            Suggestion = model.Suggestion.Trim()
         });

         return RedirectToAction(nameof(Index));
    }

    public IActionResult Select(int id)
    {
        var suggest = _userStore.GetSuggestById(id);
        if (suggest is null)
        {
            return NotFound();
        }

        return View(new SuggestionFormViewModel
        {
            SuggestId = suggest.SuggestId,
            Suggestion = suggest.Suggestion,
            MyName = suggest.MyName
        });
    }

    public IActionResult Delete(int id)
    {
        var suggest = _userStore.GetSuggestById(id);
        if (suggest is null)
        {
            return NotFound();
        }
        return View(suggest);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int suggestId)
    {
        var suggest = _userStore.GetSuggestById(suggestId);
        if (suggest is null)
        {
            return NotFound();
        }

        _userStore.DeleteSuggestion(suggestId);
        return RedirectToAction(nameof(Index));
    }
}
