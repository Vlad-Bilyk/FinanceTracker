using FinanceTracker.BlazorUI.Models.Commons;

namespace FinanceTracker.BlazorUI.Models.Operation;

public record OperationDetailsDto(
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
