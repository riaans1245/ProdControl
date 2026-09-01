using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class ProdIngredViewModel
{
    public int Id { get; set; }

    [StringLength(200)]
    [Display(Name = "Product Ingredience name")]
     public string ProdIngredienceName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

  
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    public IReadOnlyCollection<SelectListItem> AvailableProducts { get; set; } = Array.Empty<SelectListItem>();
}   