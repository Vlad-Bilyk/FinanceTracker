namespace FinanceTracker.Domain.Entities;

public class FinancialOperation
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid TypeId { get; set; }

    public decimal AmountBase { get; set; }
    public string? CurrencyOriginal { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }

    public FinancialOperationType Type { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
}
