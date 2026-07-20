using System.ComponentModel.DataAnnotations;

namespace test1233.Models;

public class RoleFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Role name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Code")]
    public int IdentityCode { get; set; }
}
