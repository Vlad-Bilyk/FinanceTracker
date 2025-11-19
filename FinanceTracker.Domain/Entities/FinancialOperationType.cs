using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class FinancialOperationType
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the operation type is an income or an expense.
    /// </summary>
    public OperationKind Kind { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; } = null!;
    public ICollection<FinancialOperation> Operations { get; set; } = [];
}
