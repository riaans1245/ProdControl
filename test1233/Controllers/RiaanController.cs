using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

public class RiaanController(ICalculationService calculationService, IUserStore userStore) : Controller
{
    private readonly ICalculationService _calculationService = calculationService;
    private readonly IUserStore _userStore = userStore;

    [AllowAnonymous]
    public IActionResult Index()
    {
        ViewData["Title"] = "Eats Dashboard";
        ViewData["DashboardLoginError"] = TempData["DashboardLoginError"];
        return View();
    }

    public IActionResult Add(int left, int right)
    {
        var result = _calculationService.Add(left, right);

        return Json(new
        {
            left,
            right,
            result
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DashboardLogin(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["DashboardLoginError"] = "Please enter a valid user name and password.";
            return RedirectToAction(nameof(Index));
        }

        var user = _userStore.ValidateUser(model.Username, model.Password);
        if (user is null)
        {
            TempData["DashboardLoginError"] = "Invalid username or password.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            TempData["DashboardLoginError"] = "Administrator access is required for Eats Dashboard.";
            return RedirectToAction(nameof(Index));
        }

        await SignInUserAsync(user, model.RememberMe);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult DashboardData()
    {
        var users = _userStore.GetAllUsersList()
            .ToDictionary(user => user.Id);

        var usedTokens = _userStore.GetAllUsedTokens()
            .OrderByDescending(token => token.SentAtUtc)
            .Select(token => new
            {
                token.Id,
                token.TokenId,
                token.Token,
                token.UserId,
                token.Username,
                token.ProductId,
                token.ProductName,
                UserFullName = users.TryGetValue(token.UserId, out var user) ? $"{user.Name} {user.Surname}".Trim() : token.Username,
                UserEmailAddress = users.TryGetValue(token.UserId, out user) ? user.EmailAddress : string.Empty,
                UserCellNo = users.TryGetValue(token.UserId, out user) ? user.CellNo : string.Empty,
                UserRole = users.TryGetValue(token.UserId, out user) ? user.Role : string.Empty,
                token.SentAtUtc
            })
            .ToList();

        var issuedTokens = _userStore.GetAllTokens()
            .Select(token => new
            {
                token.TokenId,
                token.Token,
                token.UserId,
                token.Username,
                token.ProductId,
                token.ProductName,
                UserFullName = users.TryGetValue(token.UserId, out var user) ? $"{user.Name} {user.Surname}".Trim() : token.Username,
                UserEmailAddress = users.TryGetValue(token.UserId, out user) ? user.EmailAddress : string.Empty,
                UserCellNo = users.TryGetValue(token.UserId, out user) ? user.CellNo : string.Empty,
                UserRole = users.TryGetValue(token.UserId, out user) ? user.Role : string.Empty,
                HasBeenSent = usedTokens.Any(usedToken => usedToken.TokenId == token.TokenId)
            })
            .OrderByDescending(token => token.HasBeenSent)
            .ThenBy(token => token.Username)
            .ToList();

        var bookedTablesByUser = _userStore.GetAllTables()
            .Where(table => table.UserId > 0 && !string.IsNullOrWhiteSpace(table.Username))
            .GroupBy(table => table.UserId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(table => table.BookedForUtc ?? DateTime.MaxValue)
                    .Select(table => new
                    {
                        table.TableId,
                        table.TableName,
                        table.TableNumber,
                        table.BookedForUtc
                    })
                    .ToList());

        var receiptsByOrderId = _userStore.GetAllReceipts()
            .SelectMany(receipt => receipt.OrderIds.Select(orderId => new { orderId, receipt }))
            .GroupBy(item => item.orderId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(item => item.receipt.PaidAtUtc)
                    .First()
                    .receipt);

        var orderPeople = _userStore.GetAllOrders()
            .GroupBy(order => new { order.UserId, order.Username })
            .Select(group =>
            {
                users.TryGetValue(group.Key.UserId, out var user);
                var orders = group
                    .OrderByDescending(order => order.PlacedAtUtc)
                    .Select(order =>
                    {
                        receiptsByOrderId.TryGetValue(order.OrderId, out var receipt);

                        var orderTokens = receipt?.AppliedTokens
                            .Where(token => token.OrderId == order.OrderId)
                            .ToList()
                            ?? [];

                        var discountedItems = order.Items
                            .Select(item =>
                            {
                                var itemTokens = orderTokens
                                    .Where(token =>
                                        token.ProductId == item.ProductId &&
                                        string.Equals(token.ProductName, item.Name, StringComparison.OrdinalIgnoreCase))
                                    .ToList();

                                var tokenDiscount = itemTokens.Sum(token => token.DiscountAmount);
                                var lineTotal = item.Price * item.Quantity;

                                return new
                                {
                                    item.ProductId,
                                    item.Name,
                                    item.CategoryName,
                                    item.Price,
                                    item.Quantity,
                                    LineTotal = lineTotal,
                                    TokenDiscount = tokenDiscount,
                                    ReducedLineTotal = Math.Max(0m, lineTotal - tokenDiscount),
                                    AppliedTokens = itemTokens.Select(token => new
                                    {
                                        token.OrderId,
                                        token.TokenId,
                                        token.Token,
                                        token.ProductId,
                                        token.ProductName,
                                        token.DiscountAmount
                                    }).ToList()
                                };
                            })
                            .ToList();

                        var tokenDiscountTotal = orderTokens.Sum(token => token.DiscountAmount);
                        var orderTotal = discountedItems.Sum(item => item.LineTotal);

                        return new
                        {
                            order.OrderId,
                            order.UserId,
                            order.Username,
                            order.PlacedAtUtc,
                            order.IsPaid,
                            PaymentStatus = order.IsPaid ? "Paid" : "Open",
                            ItemCount = order.Items.Sum(item => item.Quantity),
                            OrderTotal = orderTotal,
                            TokenDiscountTotal = tokenDiscountTotal,
                            ReducedOrderTotal = Math.Max(0m, orderTotal - tokenDiscountTotal),
                            ReceiptNumber = receipt?.ReceiptNumber ?? string.Empty,
                            PaidAtUtc = receipt?.PaidAtUtc,
                            AppliedTokens = orderTokens.Select(token => new
                            {
                                token.OrderId,
                                token.TokenId,
                                token.Token,
                                token.ProductId,
                                token.ProductName,
                                token.DiscountAmount
                            }).ToList(),
                            Items = discountedItems
                        };
                    })
                    .ToList();

                var tableBookings = bookedTablesByUser.TryGetValue(group.Key.UserId, out var userTables)
                    ? userTables
                    : [];

                return new
                {
                    group.Key.UserId,
                    group.Key.Username,
                    UserFullName = user is null ? group.Key.Username : $"{user.Name} {user.Surname}".Trim(),
                    UserEmailAddress = user?.EmailAddress ?? string.Empty,
                    UserCellNo = user?.CellNo ?? string.Empty,
                    UserRole = user?.Role ?? string.Empty,
                    TotalOrders = orders.Count,
                    TotalOpenOrders = orders.Count(order => !order.IsPaid),
                    TotalPaidOrders = orders.Count(order => order.IsPaid),
                    TotalItems = orders.Sum(order => order.ItemCount),
                    TotalValue = orders.Sum(order => order.OrderTotal),
                    HasTableBooking = tableBookings.Count > 0,
                    TableBookings = tableBookings,
                    Orders = orders
                };
            })
            .OrderByDescending(person => person.TotalOrders)
            .ThenBy(person => person.UserFullName)
            .ToList();

        return Json(new
        {
            generatedAtUtc = DateTime.UtcNow,
            currentUser = User.Identity?.Name ?? string.Empty,
            summary = new
            {
                issuedTokenCount = issuedTokens.Count,
                usedTokenCount = usedTokens.Count(),
                uniqueUsersWithSentTokens = usedTokens
                    .Select(token => token.UserId)
                    .Distinct()
                    .Count(),
                currentOpenOrderCount = orderPeople.Sum(person => person.TotalOpenOrders),
                paidOrderCount = orderPeople.Sum(person => person.TotalPaidOrders),
                peopleWithOrders = orderPeople.Count,
                peopleWithCurrentOrders = orderPeople.Count(person => person.TotalOpenOrders > 0),
                bookedTableCount = bookedTablesByUser.Sum(group => group.Value.Count),
                peopleWithBookedTables = bookedTablesByUser.Count,
                latestTokenSentAtUtc = usedTokens.FirstOrDefault()?.SentAtUtc
            },
            activityEndpoint = Url.Action("GetUsedTokens", "TokenApi"),
            issuedTokens,
            usedTokens,
            currentOrders = orderPeople,
            latestUsedToken = usedTokens.FirstOrDefault()
        });
    }

    private async Task SignInUserAsync(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent
            });
    }
}
