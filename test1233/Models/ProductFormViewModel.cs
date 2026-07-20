using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    [Display(Name = "Price ($)")]
    public decimal Price { get; set; }

    public IReadOnlyCollection<SelectListItem> AvailableCategories { get; set; } = Array.Empty<SelectListItem>();
}
