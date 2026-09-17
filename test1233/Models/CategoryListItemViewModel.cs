namespace test1233.Models;

public class CategoryListItemViewModel
{
    public int Id { get; set; }

    public required string CatName { get; set; }

    public string CatDescription { get; set; } = string.Empty;

    public int ProductCount { get; set; }

    public IReadOnlyCollection<AppProduct> Products { get; set; } = Array.Empty<AppProduct>();
}
