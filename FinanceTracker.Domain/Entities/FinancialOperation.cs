namespace FinanceTracker.Domain.Entities;

public class FinancialOperation
{
    public Guid Id { get; set; }
    public Guid TypeId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }

    public FinancialOperationType Type { get; set; } = null!;
}
