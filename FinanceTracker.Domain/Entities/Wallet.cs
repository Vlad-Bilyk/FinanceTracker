namespace FinanceTracker.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Wallet base currency ISO-4217 code (e.g., "USD").
    /// </summary>
    public string BaseCurrencyCode { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public Currency BaseCurrency { get; set; } = null!;
    public ICollection<FinancialOperation> Operations { get; set; } = [];
}
