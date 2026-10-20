using test1233.Models;

namespace test1233.Services;

public interface ILoyaltyVoucherEmailSender
{
    bool TrySend(AppUser user, string voucherCode);
}
