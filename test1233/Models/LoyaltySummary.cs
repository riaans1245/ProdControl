namespace test1233.Models;

public class LoyaltySummary
{
    public int AvailablePoints { get; init; }

    public int PointsUntilVoucher => 100 - AvailablePoints;
}
