namespace test1233.Models;

public class RoleIndexViewModel
{
    public IReadOnlyCollection<AppRole> Roles { get; init; } = Array.Empty<AppRole>();

    public IReadOnlyDictionary<int, int> UserCountsByRoleId { get; init; } = new Dictionary<int, int>();
}
