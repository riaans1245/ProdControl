using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class NotifiFormViewModel
{
    public int NotificationId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Notification")]
    public string Notification { get; set; } = string.Empty;

    [Required]
    [Display(Name = "User")]
    public int UserId { get; set; }

    public bool AllUsers{get; set;}

    public IReadOnlyCollection<SelectListItem> AvailableUsers { get; set; } = Array.Empty<SelectListItem>();
}
