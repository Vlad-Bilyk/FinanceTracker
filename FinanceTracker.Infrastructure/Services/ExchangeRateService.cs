using FinanceTracker.Application.DTOs.ExchangeRate;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FinanceTracker.Infrastructure.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly string _apiKey;

    public ExchangeRateService(HttpClient httpClient, ILogger<ExchangeRateService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiKey = configuration["FreeCurrencyAPI:ApiKey"]
            ?? throw new InvalidOperationException("Exchange Rate API Key not configured");
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency,
        DateTime date, CancellationToken ct = default)
    {
        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        const decimal baseExchangeRate = 1m;
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("FromCurrency and ToCurrency are the same ({Currency}). Returning base exchange rate of 1.0",
                fromCurrency);
            return baseExchangeRate;
        }

        if (date.Date == DateTime.Now.Date)
        {
            return await GetTodayExchangeRateAsync(fromCurrency, toCurrency, ct);
        }
        else
        {
            return await GetHistoricalExchangeRateAsync(fromCurrency, toCurrency, date, ct);
        }
    }

    private async Task<decimal> GetTodayExchangeRateAsync(string fromCurrency, string toCurrency, CancellationToken ct)
    {
        var requestUri = $"https://api.freecurrencyapi.com/v1/latest?apikey={_apiKey}&currencies={toCurrency}&base_currency={fromCurrency}";

        var json = await ExecuteHttpRequestAsync(requestUri, ct);
        var data = JsonSerializer.Deserialize<LatestRateResponse>(json);

        if (data == null || !data.Data.TryGetValue(toCurrency, out decimal rate))
        {
            throw new InvalidOperationException("Exchange rate data is unavailable.");
        }

        _logger.LogInformation("Fetched historical rate: {From}->{To} = {Rate}",
            fromCurrency, toCurrency, rate);
        return rate;
    }

    private async Task<decimal> GetHistoricalExchangeRateAsync(string fromCurrency, string toCurrency,
        DateTime date, CancellationToken ct)
    {
        var dateString = date.ToString("yyyy-MM-dd");
        var requestUri = $"https://api.freecurrencyapi.com/v1/historical?apikey={_apiKey}&date={dateString}&currencies={toCurrency}&base_currency={fromCurrency}";

        var json = await ExecuteHttpRequestAsync(requestUri, ct);
        var data = JsonSerializer.Deserialize<HistoricalRateResponse>(json);

        if (data == null || !data.Data.TryGetValue(dateString, out Dictionary<string, decimal>? rates))
        {
            throw new InvalidOperationException("Historical exchange rates data is unavailable.");
        }

        if (rates == null || !rates.TryGetValue(toCurrency, out decimal rate))
        {
            throw new InvalidOperationException("Historical exchange rate data is unavailable for the specified currency.");
        }

        _logger.LogInformation("Fetched historical exchange rate: {From}->{To} on {Date} = {Rate}",
            fromCurrency, toCurrency, dateString, rate);
        return rate;
    }

    private async Task<string> ExecuteHttpRequestAsync(string requestUri, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }
}
