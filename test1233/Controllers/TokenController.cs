using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class TokenController(IUserStore userStore) : Controller
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
        return View(_userStore.GetAllTokens());
    }

    public IActionResult Create()
    {
        return View(new TokenFormViewModel
        {
            AvailableUsers = GetUserSelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TokenFormViewModel model)
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

        if (_userStore.TokenNameExists(model.Token, model.UserId))
        {
            ModelState.AddModelError(nameof(model.Token), "That Token already exists in the selected category.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            return View(model);
        }

        _userStore.CreateToken(new AppTokens
        {
             Token = model.Token.Trim(),
             UserId = user!.Id,
             Username = user.Username
        });

        return RedirectToAction(nameof(Index));
    }

     public IActionResult Edit(int id)
    {
        var token = _userStore.GetTokenById(id);
        if (token is null)
        {
            return NotFound();
        }

         return View(new TokenFormViewModel
         {
             TokenId = token.TokenId,
             Token = token.Token,
             UserId = token.UserId,
             AvailableUsers = GetUserSelectList()
             
         });
    }

    private IReadOnlyCollection<SelectListItem> GetUserSelectList()
    {
        return _userStore.GetAllUsers()
            .Select(user => new SelectListItem(user.Username, user.Id.ToString()))
            .ToList();
    }
}
