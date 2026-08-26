using System.Collections.Generic;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace test1233.Controllers;


public class UserNavigationController(IUserStore userStore, ITokenApiClient tokenApiClient) : AppController(userStore)
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

    public IActionResult UserOrderList(string searchString, int page = 1)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var pendingOrders = _userStore.GetPendingOrdersForUser(currentUser.Id);
        var rows = pendingOrders
            .SelectMany(order => order.Items.Select(item => new UserOrderRowViewModel
            {
                OrderId = order.OrderId,
                PlacedAtUtc = order.PlacedAtUtc,
                Name = item.Name,
                CategoryName = item.CategoryName,
                Quantity = item.Quantity,
                Price = item.Price
            }));

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var normalizedSearch = searchString.Trim();
            rows = rows.Where(row =>
                row.OrderId.ToString().Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                row.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                row.CategoryName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        var filteredRows = rows.ToList();
        var totalItems = filteredRows.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)PageSize);
        var pageNumber = Math.Min(Math.Max(1, page), totalPages);
        var items = filteredRows
            .Skip((pageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList()
            .AsReadOnly();

        var pendingOrderCount = pendingOrders
            .SelectMany(order => order.Items)
            .Sum(item => item.Quantity);

        var pendingOrderTotal = pendingOrders
            .SelectMany(order => order.Items)
            .Sum(item => item.Quantity * item.Price);

        return View(new UserOrderListViewModel
        {
            Orders = new PagedListViewModel<UserOrderRowViewModel>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = PageSize,
                TotalItems = totalItems
            },
            SearchString = searchString ?? string.Empty,
            PendingOrderCount = pendingOrderCount,
            PendingOrderTotal = pendingOrderTotal
        });
    }

    public IActionResult UserOrderPlace()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(BuildUserOrderPlaceViewModel(currentUser));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmPayment()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var receipt = _userStore.ConfirmPendingOrdersPayment(currentUser.Id, currentUser.Username, DateTime.UtcNow);
        if (receipt is null)
        {
            TempData["OrderPlaceMessage"] = "There are no order items ready for payment.";
            return RedirectToAction(nameof(UserOrderPlace));
        }

        TempData["OrderPlaceMessage"] = $"Payment recorded successfully. Receipt {receipt.ReceiptNumber} was generated.";
        return View("UserOrderPlace", new UserOrderPlaceViewModel
        {
            PendingOrders = _userStore.GetPendingOrdersForUser(currentUser.Id),
            LatestReceipt = receipt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteBooking(int tableId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var table = _userStore.GetTablesById(tableId);
        if (table is null)
        {
            return NotFound();
        }

        if (table.UserId != currentUser.Id)
        {
            return NotFound();
        }

        _userStore.UpdateTable(new AppTables
        {
            TableId = table.TableId,
            TableName = table.TableName,
            TableNumber = table.TableNumber,
            UserId = 0,
            Username = string.Empty,
            BookedForUtc = null
        });

        return RedirectToAction(nameof(UserOrderTable));
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

    private UserOrderPlaceViewModel BuildUserOrderPlaceViewModel(AppUser currentUser)
    {
        return new UserOrderPlaceViewModel
        {
            PendingOrders = _userStore.GetPendingOrdersForUser(currentUser.Id),
            LatestReceipt = _userStore.GetLatestReceiptForUser(currentUser.Id)
        };
    }

     public IActionResult UserOrderTable()
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(_userStore.GetTablesGroupedByUser(currentUser.Username));
    }

     public IActionResult UserBookMe(int TableId)
     {
         var currentUser = GetCurrentUser();
         if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

         var tables = _userStore.GetTablesById(TableId);
        if (tables is null)
        {
            return NotFound();
        }

        return View(new TableFormViewModel
        {
            TableId = tables.TableId,
            TableName = tables.TableName,
            TableNumber = tables.TableNumber,
            UserId = currentUser.Id,
            BookedForUtc = tables.BookedForUtc
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UserBookMe(TableFormViewModel model)
    {
        var currentUser = GetCurrentUser();
        if (currentUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var tables = _userStore.GetTablesById(model.TableId);
        if (tables is null)
        {
            return NotFound();
        }

        model.UserId = currentUser.Id;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!model.BookedForUtc.HasValue)
        {
            ModelState.AddModelError(nameof(model.BookedForUtc), "Please select a booking date and time.");
            return View(model);
        }

        if (_userStore.TableNameExists(model.TableName, model.TableId))
        {
            ModelState.AddModelError(nameof(model.TableId), "That table already exists.");
            return View(model);
        }

        _userStore.UpdateTable(new AppTables
        {
            TableId = model.TableId,
            TableName = model.TableName.Trim(),
            TableNumber = model.TableNumber,
            UserId = currentUser.Id,
            Username = currentUser.Username,
            BookedForUtc = model.BookedForUtc
        });

        return RedirectToAction(nameof(UserOrderTable));
    }
}
