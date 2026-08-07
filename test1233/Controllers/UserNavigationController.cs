using System.Collections.Generic;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;


public class UserNavigationController(IUserStore userStore, ITokenApiClient tokenApiClient) : Controller
{
    private readonly IUserStore _userStore = userStore;
    private readonly ITokenApiClient _tokenApiClient = tokenApiClient;
    private const int PageSize = 10;


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserTokenList(string searchString, int page = 1)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["CurrentSearch"] = searchString;

        var tokens = _userStore.GetAllTokens()
            .Where(token => token.UserId == currentUser.Id);

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            tokens = tokens.Where(token =>
                token.Token.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

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

    public IActionResult UserNotificationsList(string searchString, int page = 1)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewData["CurrentSearch"] = searchString;

        var notifications = _userStore.GetAllNotifications()
            .Where(notification => notification.UserId == currentUser.Id);

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            notifications = notifications.Where(notification =>
                notification.Notification.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                notification.UserName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

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

    public IActionResult UserNotificationDelete(int id)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var notification = _userStore.GetNotificationById(id);
        if (notification is null || notification.UserId != currentUser.Id)
        {
            return NotFound();
        }

        return View(notification);
    }

    [HttpPost, ActionName("UserNotificationDelete")]
    [ValidateAntiForgeryToken]
    public IActionResult UserNotificationDeleteConfirmed(int notificationId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var notification = _userStore.GetNotificationById(notificationId);
        if (notification is null || notification.UserId != currentUser.Id)
        {
            return NotFound();
        }

        _userStore.DeleteNotification(notificationId);
        return RedirectToAction(nameof(UserNotificationsList));
    }

    public IActionResult UserOrderList()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }

    public IActionResult UserOrderPlace()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmPayment([FromBody] OrderPaymentRequest request)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return Unauthorized(new { success = false, message = "You must be signed in to confirm payment." });
        }

        if (request is null || request.Items.Count == 0)
        {
            return BadRequest(new { success = false, message = "No order items were sent for payment." });
        }

        var normalizedTotalItems = request.Items.Sum(item => Math.Max(0, item.Quantity));
        var normalizedTotalValue = request.Items.Sum(item => Math.Max(0, item.Quantity) * Math.Max(0m, item.Price));

        return Json(new
        {
            success = true,
            message = "Payment recorded successfully.",
            paidBy = currentUser.Username,
            receiptNumber = request.ReceiptNumber,
            totalItems = normalizedTotalItems,
            totalValue = normalizedTotalValue,
            paidAtUtc = request.PaidAtUtc
        });
    }

    public IActionResult Products()
    {
        return View(_userStore.GetAllProducts());
    }

    public IActionResult Categories()
    {
        return View(_userStore.GetAllCategories());
    }

    public IActionResult ContactUs()
    {
        return View(_userStore.GetAllContactUs());
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

    public IActionResult Use(int id)
    {
         var currentUser = GetCurrentUser();
         if (currentUser is null)
         {
             return RedirectToAction("Login", "Account");
         }

         var tokens = _userStore.GetTokenById(id);
         if (tokens is null || tokens.UserId != currentUser.Id)
         {
             return NotFound();
         }

        return View(tokens);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Use(UseTokenApiRequest request, CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var token = _userStore.GetTokenById(request.TokenId);
        if (token is null || token.UserId != currentUser.Id)
        {
            return NotFound();
        }

        request.UserId = token.UserId;
        request.Username = token.Username;
        request.Token = token.Token;

        var apiResponse = await _tokenApiClient.SendUsedTokenAsync(
            GetBaseUrl(),
            Request.Headers.Cookie.ToString(),
            request,
            cancellationToken);

        TempData["TokenApiMessage"] = apiResponse.Message;

        if (!apiResponse.Success)
        {
            return View(token);
        }

        return RedirectToAction(nameof(UserTokenList));
    }

    private string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}";
    }
}
