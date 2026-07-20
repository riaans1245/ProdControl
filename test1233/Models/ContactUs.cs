using System.ComponentModel.DataAnnotations;
namespace test1233.Models;

public class ContactUs
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public required string Name { get; set; }

    [Required]
    [StringLength(50)]
    public required string Surname { get; set; }

    [Required]
    [StringLength(60)]
    public required string EmailAddress { get; set; }

    [Required]
    [StringLength(20)]
    public required string CellNo { get; set; }
}
