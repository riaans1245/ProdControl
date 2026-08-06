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


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserTokenList()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var tokens = _userStore.GetAllTokens()
            .Where(token => token.UserId == currentUser.Id)
            .ToList()
            .AsReadOnly();

        return View(tokens);
    }

    public IActionResult UserNotificationsList()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var notifications = _userStore.GetAllNotifications()
            .Where(notification => notification.UserId == currentUser.Id)
            .ToList()
            .AsReadOnly();

        return View(notifications);
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
