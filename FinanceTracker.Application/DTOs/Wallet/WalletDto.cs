namespace FinanceTracker.Application.DTOs.Wallet;

public record WalletDto(
    Guid Id,
    string Name,
    string BaseCurrencyCode
);
