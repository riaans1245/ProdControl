namespace test1233.Models;

public class AppProduct
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int CategoryId { get; set; }

    public required string CategoryName { get; set; }
}
