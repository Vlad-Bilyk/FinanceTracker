using FinanceTracker.BlazorUI.Models.Commons;

namespace FinanceTracker.BlazorUI.Models.OperationType;

public class OperationTypeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OperationKind Kind { get; set; }
}
