namespace test1233.Models;

public class AppUser
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }

    public required string EmailAddress { get; set; }

    public required string CellNo { get; set; }

    public required string Password { get; set; }

    public int RoleId { get; set; }

    public required string Role { get; set; }
}
