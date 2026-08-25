using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class TableFormViewModel
{
    public int TableId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Table Name")]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Table Number")]
    public int TableNumber { get; set; }

    [Display(Name = "User")]
    public int? UserId { get; set; }

    public IReadOnlyCollection<SelectListItem> AvailableUsers { get; set; } = Array.Empty<SelectListItem>();


}
