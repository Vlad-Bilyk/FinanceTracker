using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public record OperationTypeDto(
    Guid Id,
    string Name,
    string Description,
    OperationKind Kind
);
