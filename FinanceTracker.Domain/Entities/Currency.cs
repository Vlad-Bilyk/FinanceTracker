namespace FinanceTracker.Domain.Entities;

public class Currency
{
    /// <summary>
    /// ISO-4217 currency code (e.g., "USD")
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Currency name (e.g., "United States Dollar")
    /// </summary>
    public string Name { get; set; } = null!;
}
