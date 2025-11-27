using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces.Services;

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyDto>> GetAllCurrenciesAsync(CancellationToken ct = default);
    Task<CurrencyDto> GetByCodeAsync(string code, CancellationToken ct = default);
}
