namespace test1233.Models;

public class LoyaltyAdminViewModel
{
    public IReadOnlyCollection<LoyaltyAdminUserRowViewModel> Users { get; init; } = Array.Empty<LoyaltyAdminUserRowViewModel>();

    public int TotalPointsReceived => Users.Sum(user => user.TotalPointsReceived);

    public int TotalVouchersIssued => Users.Sum(user => user.VouchersReceived);
}

public class LoyaltyAdminUserRowViewModel
{
    public int UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string EmailAddress { get; init; } = string.Empty;

    public int TotalPointsReceived { get; init; }

    public int VouchersReceived { get; init; }

    public int PointsLeft { get; init; }
}
