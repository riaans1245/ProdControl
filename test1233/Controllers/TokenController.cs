using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class TokenController(IUserStore userStore, ITokenApiClient tokenApiClient) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;
    private readonly ITokenApiClient _tokenApiClient = tokenApiClient;
    private const int PageSize = 10;

    public IActionResult Index(string searchString, string sortOrder = "", int page = 1)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentSort"] = sortOrder;
        ViewData["TokenSort"] = sortOrder == "token_asc" ? "token_desc" : "token_asc";
        ViewData["UserSort"] = sortOrder == "user_asc" ? "user_desc" : "user_asc";
        ViewData["ProductSort"] = sortOrder == "product_asc" ? "product_desc" : "product_asc";

        var tokens = from token in _userStore.GetAllTokens()
                     select token;

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            tokens = tokens.Where(token =>
                token.Token.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                token.Username.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                token.ProductName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                token.UserId.ToString().Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        tokens = sortOrder switch
        {
            "token_asc" => tokens
                .OrderBy(token => token.Token)
                .ThenBy(token => token.Username)
                .ThenBy(token => token.TokenId),
            "token_desc" => tokens
                .OrderByDescending(token => token.Token)
                .ThenBy(token => token.Username)
                .ThenBy(token => token.TokenId),
            "user_asc" => tokens
                .OrderBy(token => token.Username)
                .ThenBy(token => token.Token)
                .ThenBy(token => token.TokenId),
            "user_desc" => tokens
                .OrderByDescending(token => token.Username)
                .ThenBy(token => token.Token)
                .ThenBy(token => token.TokenId),
            "product_asc" => tokens
                .OrderBy(token => token.ProductName)
                .ThenBy(token => token.Token)
                .ThenBy(token => token.TokenId),
            "product_desc" => tokens
                .OrderByDescending(token => token.ProductName)
                .ThenBy(token => token.Token)
                .ThenBy(token => token.TokenId),
            _ => tokens
                .OrderBy(token => token.TokenId)
        };

        var filteredTokens = tokens.ToList();
        var totalItems = filteredTokens.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredTokens
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        return View(new PagedListViewModel<AppTokens>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = PageSize,
            TotalItems = totalItems
        });
    }

    public async Task<IActionResult> Used(CancellationToken cancellationToken)
    {
        var usedTokens = await _tokenApiClient.GetUsedTokensAsync(
            GetBaseUrl(),
            Request.Headers.Cookie.ToString(),
            cancellationToken);

        return View(usedTokens);
    }

    public IActionResult Create()
    {
        return View(new TokenFormViewModel
        {
            AvailableUsers = GetUserSelectList(),
            AvailableProducts = GetProductSelectList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TokenFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            model.AvailableProducts = GetProductSelectList();
            return View(model);
        }

        var user = _userStore.GetUserById(model.UserId);
        var product = _userStore.GetProductById(model.ProductId);
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.UserId), "Please choose a valid user.");
        }

        if (product is null)
        {
            ModelState.AddModelError(nameof(model.ProductId), "Please choose a valid product.");
        }

        if (_userStore.TokenNameExists(model.Token, model.UserId))
        {
            ModelState.AddModelError(nameof(model.Token), "That token already exists for the selected user.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            model.AvailableProducts = GetProductSelectList();
            return View(model);
        }

        _userStore.CreateToken(new AppTokens
        {
            Token = model.Token.Trim(),
            UserId = user!.Id,
            Username = user.Username,
            ProductId = product!.Id,
            ProductName = product.Name
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
            ProductId = token.ProductId,
            AvailableUsers = GetUserSelectList(),
            AvailableProducts = GetProductSelectList()
         });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(TokenFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            model.AvailableProducts = GetProductSelectList();
            return View(model);
        }

        var existingToken = _userStore.GetTokenById(model.TokenId);
        if (existingToken is null)
        {
            return NotFound();
        }

        var user = _userStore.GetUserById(model.UserId);
        var product = _userStore.GetProductById(model.ProductId);
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.UserId), "Please choose a valid user.");
        }

        if (product is null)
        {
            ModelState.AddModelError(nameof(model.ProductId), "Please choose a valid product.");
        }

        if (_userStore.TokenNameExists(model.Token, model.UserId, model.TokenId))
        {
            ModelState.AddModelError(nameof(model.Token), "That token already exists for the selected user.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableUsers = GetUserSelectList();
            model.AvailableProducts = GetProductSelectList();
            return View(model);
        }

        _userStore.UpdateToken(new AppTokens
        {
            TokenId = model.TokenId,
            Token = model.Token.Trim(),
            UserId = user!.Id,
            Username = user.Username,
            ProductId = product!.Id,
            ProductName = product.Name
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var token = _userStore.GetTokenById(id);
        if (token is null)
        {
            return NotFound();
        }

        return View(token);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int tokenId)
    {
        var token = _userStore.GetTokenById(tokenId);
        if (token is null)
        {
            return NotFound();
        }

        _userStore.DeleteToken(tokenId);
        return RedirectToAction(nameof(Index));
    }

    private IReadOnlyCollection<SelectListItem> GetUserSelectList()
    {
        return _userStore.GetAllUsers()
            .Select(user => new SelectListItem(user.Username, user.Id.ToString()))
            .ToList();
    }

    private IReadOnlyCollection<SelectListItem> GetProductSelectList()
    {
        return _userStore.GetAllProducts()
            .Select(product => new SelectListItem(product.Name, product.Id.ToString()))
            .ToList();
    }

    private string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}";
    }
}
