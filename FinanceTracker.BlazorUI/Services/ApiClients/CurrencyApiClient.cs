using FinanceTracker.BlazorUI.Models;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class CurrencyApiClient
{
    private readonly HttpClient _httpClient;
    private const string _baseUrl = "api/currencies";

    public CurrencyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<CurrencyDto>>(_baseUrl, ct);
        return result ?? [];
    }
}
