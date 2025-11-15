using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs.Operation;

public record FinancialOperationDetailsDto(
    Guid Id,
    Guid TypeId,
    string TypeName,
    OperationKind Kind,
    Guid WalletId,
    string WalletName,
    decimal AmountBase,
    decimal AmountOriginal,
    string? CurrencyOriginalCode,
    DateTime Date,
    string? Note
);
