namespace FinanceTracker.BlazorUI.Models.Commons;

public class ProblemDetailsResponse
{
    public string? Type { get; set; }

    public string? Title { get; set; }

    public int? Status { get; set; }

    public string? Detail { get; set; }

    public IDictionary<string, string[]>? Errors { get; set; } = new Dictionary<string, string[]>();
}
