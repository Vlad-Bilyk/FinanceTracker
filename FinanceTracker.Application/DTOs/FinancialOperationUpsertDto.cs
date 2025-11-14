namespace FinanceTracker.Application.DTOs;

public record FinancialOperationUpsertDto(
    Guid TypeId,
    decimal Amount,
    DateTime Date,
    string? Note
);
