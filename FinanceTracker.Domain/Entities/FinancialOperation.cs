namespace FinanceTracker.Domain.Entities;

public class FinancialOperation
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid TypeId { get; set; }

    /// <summary>
    /// Amount in the wallet’s base currency after conversion.
    /// </summary>
    /// <remarks>
    /// Calculated from <see cref="AmountOriginal"/> using the applied exchange rate
    /// </remarks> 
    public decimal AmountBase { get; set; }

    /// <summary>
    /// User-entered amount in the original currency.
    /// </summary>
    public decimal AmountOriginal { get; set; }

    /// <summary>
    /// ISO-4217 currency code of the original amount (e.g., "USD").
    /// </summary>
    /// <remarks>
    /// If <c>null</c>, the operation is recorded in the wallet’s base currency.
    /// </remarks>
    public string? CurrencyOriginalCode { get; set; }

    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }

    public FinancialOperationType Type { get; set; } = null!;
    public Currency? Currency { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
}
