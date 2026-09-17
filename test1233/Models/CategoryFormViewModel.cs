using System.ComponentModel.DataAnnotations;

namespace test1233.Models;

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Category description")]
    public string Description { get; set; } = string.Empty;
}
