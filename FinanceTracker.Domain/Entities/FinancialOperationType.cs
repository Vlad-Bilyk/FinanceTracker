using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Entities;

public class FinancialOperationType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OperationKind Kind { get; set; }

    public ICollection<FinancialOperation> Operations { get; set; } = [];
}
