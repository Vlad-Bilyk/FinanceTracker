namespace FinanceTracker.BlazorUI.Models.Wallet;

public record WalletDto(
    Guid Id,
    string Name,
    string BaseCurrencyCode
);
