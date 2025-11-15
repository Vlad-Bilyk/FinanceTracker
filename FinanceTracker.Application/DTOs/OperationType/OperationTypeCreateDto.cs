using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs.OperationType;

public class OperationTypeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OperationKind Kind { get; set; }
}
