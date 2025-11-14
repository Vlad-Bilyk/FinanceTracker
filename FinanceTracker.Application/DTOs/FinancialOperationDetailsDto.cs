using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public record FinancialOperationDetailsDto(
    Guid Id,
    Guid TypeId,
    string TypeName,
    OperationKind Kind,
    decimal Amount,
    DateTime Date,
    string? Note
);
