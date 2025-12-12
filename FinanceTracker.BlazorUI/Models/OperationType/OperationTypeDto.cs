using FinanceTracker.BlazorUI.Models.Commons;

namespace FinanceTracker.BlazorUI.Models.OperationType;

public record OperationTypeDto(
    Guid Id,
    string Name,
    string Description,
    OperationKind Kind
);
