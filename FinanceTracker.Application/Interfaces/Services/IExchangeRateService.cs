namespace FinanceTracker.Application.Interfaces.Services;

public interface IExchangeRateService
{
    Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency,
        DateTime date, CancellationToken ct = default);
}
