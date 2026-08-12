using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;
public class SuggestionController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, int page = 1)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        ViewData["CurrentSearch"] = searchString;

         var suggestions = from suggestion in _userStore.GetAllSuggestions()
                           select suggestion;

         if (!string.IsNullOrWhiteSpace(searchString))
         {
             var normalizedSearch = searchString.Trim();
             suggestions = suggestions.Where(suggestion =>
                 (suggestion.MyName ?? string.Empty).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                 suggestion.Suggestion.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
         }

         var filteredSuggestions = suggestions.ToList();
         var totalItems = filteredSuggestions.Count;
         var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
         var pageNumber = Math.Min(Math.Max(1, page), totalPages);
         var items = filteredSuggestions
             .Skip((pageNumber - 1) * PageSize)
             .Take(PageSize)
             .ToList()
             .AsReadOnly();

         return View(new PagedListViewModel<AppSuggestion>
         {
             Items = items,
             PageNumber = pageNumber,
             PageSize = PageSize,
             TotalItems = totalItems
         });
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
