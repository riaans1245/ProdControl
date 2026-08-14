using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class TokenFormViewModel
{
    public int TokenId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Token name")]
    public string Token { get; set; } = string.Empty;

    [Required]
    [Display(Name = "User")]
    public int UserId { get; set; }

    [Required]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    public IReadOnlyCollection<SelectListItem> AvailableUsers { get; set; } = Array.Empty<SelectListItem>();

    public IReadOnlyCollection<SelectListItem> AvailableProducts { get; set; } = Array.Empty<SelectListItem>();
}
