using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test1233.Models;
using test1233.Services;

namespace test1233.Controllers;

[Authorize(Roles = "Admin")]
public class LoyaltyController(IUserStore userStore) : AppController(userStore)
{
    private readonly IUserStore _userStore = userStore;

    public IActionResult Index()
    {
        var receiptsByUser = _userStore.GetAllReceipts()
            .GroupBy(receipt => receipt.UserId)
            .ToDictionary(group => group.Key, group => new
            {
                Points = group.Sum(receipt => receipt.LoyaltyPointsEarned),
                Vouchers = group.Sum(receipt => receipt.IssuedVoucherCodes.Count)
            });

        var users = _userStore.GetAllUsers()
            .OrderBy(user => user.Username)
            .Select(user =>
            {
                receiptsByUser.TryGetValue(user.Id, out var activity);
                return new LoyaltyAdminUserRowViewModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    EmailAddress = user.EmailAddress,
                    TotalPointsReceived = activity?.Points ?? 0,
                    VouchersReceived = activity?.Vouchers ?? 0,
                    PointsLeft = _userStore.GetLoyaltySummary(user.Id).AvailablePoints
                };
            })
            .ToList()
            .AsReadOnly();

        return View(new LoyaltyAdminViewModel { Users = users });
    }
}
