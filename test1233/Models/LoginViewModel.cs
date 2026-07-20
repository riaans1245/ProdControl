using System.ComponentModel.DataAnnotations;

namespace test1233.Models;

public class LoginViewModel
{
    [Required]
    [Display(Name = "User name")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }

    [Display(Name = "Send me a magic link")]
    public bool RequestMagicLink { get; set; }
}
