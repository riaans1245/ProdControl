using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace test1233.Models;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "User name")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Surname")]
    public string Surname { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Cell no")]
    public string CellNo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public int RoleId { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public IReadOnlyCollection<SelectListItem> AvailableRoles { get; set; } = Array.Empty<SelectListItem>();
}
