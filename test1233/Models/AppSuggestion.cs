namespace test1233.Models;

public class AppSuggestion
{
    public int SuggestId { get; set; }

    public required string Suggestion { get; set; }

    public string? MyName { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.Now;
}