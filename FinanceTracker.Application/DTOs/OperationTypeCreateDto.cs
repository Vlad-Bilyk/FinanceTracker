using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public record OperationTypeCreateDto(
    string Name,
    string Description,
    OperationKind Kind
);
