using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.DTOs.ExchangeRate;
using FinanceTracker.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FinanceTracker.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICurrencyCatalogProvider"/> that fetches
/// currencies from the FreeCurrencyAPI service.
/// </summary>
public class ExternalCurrencyCatalogProvider : ICurrencyCatalogProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalCurrencyCatalogProvider> _logger;
    private readonly string _apiKey;

    public ExternalCurrencyCatalogProvider(HttpClient httpClient, IConfiguration configuration,
        ILogger<ExternalCurrencyCatalogProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiKey = configuration["FreeCurrencyAPI:ApiKey"]
            ?? throw new InvalidOperationException("Exchange Rate API Key not configured");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CurrencyDto>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        var requestUri = $"https://api.freecurrencyapi.com/v1/currencies?apikey={_apiKey}";

        var response = await _httpClient.GetAsync(requestUri, ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);

        var data = JsonSerializer.Deserialize<CurrenciesResponse>(json)
            ?? throw new InvalidOperationException("Currencies list is unavailable.");

        var currencies = data.Data.Values.ToList();

        _logger.LogInformation("Fetched {Count} currencies from FreeCurrencyAPI", currencies.Count);

        return currencies
            .Select(x => new CurrencyDto(x.Code, x.Name))
            .ToList();
    }
}
