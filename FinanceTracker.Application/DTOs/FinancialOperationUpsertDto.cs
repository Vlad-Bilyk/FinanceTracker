namespace FinanceTracker.Application.DTOs;

public record FinancialOperationUpsertDto(
    Guid TypeId,
    decimal Amount,
    DateTimeOffset Date,
    string? Note
);
