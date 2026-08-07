namespace test1233.Models;

public class RoleIndexViewModel
{
    public IReadOnlyCollection<AppRole> Roles { get; init; } = Array.Empty<AppRole>();

    public IReadOnlyDictionary<int, int> UserCountsByRoleId { get; init; } = new Dictionary<int, int>();

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
