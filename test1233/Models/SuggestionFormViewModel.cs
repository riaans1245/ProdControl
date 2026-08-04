using System.ComponentModel.DataAnnotations;

namespace test1233.Models;

public class SuggestionFormViewModel
{
    public int SuggestId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Suggestion")]
    public required string Suggestion { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "My Name")]
    public string? MyName { get; set; } = string.Empty;
}
