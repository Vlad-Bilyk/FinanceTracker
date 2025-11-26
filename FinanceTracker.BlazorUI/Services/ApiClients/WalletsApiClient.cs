using FinanceTracker.BlazorUI.Models.Commons;
using FinanceTracker.BlazorUI.Models.Wallet;
using FinanceTracker.BlazorUI.Services.Commons;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class WalletsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiErrorHandler _apiErrorHandler;
    private const string _basePath = "api/wallets";

    public WalletsApiClient(HttpClient httpClient, IApiErrorHandler  apiErrorHandler)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiErrorHandler = apiErrorHandler ?? throw new ArgumentNullException(nameof(apiErrorHandler));
    }

    public async Task<WalletDto?> GetWalletByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<WalletDto>($"{_basePath}/{id}", ct);
        return result;
    }

    public async Task<IReadOnlyList<WalletDto>> GetUserWalletsAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<WalletDto>>(_basePath, ct);
        return result ?? [];
    }

    public async Task<ApiResult> CreateAsync(WalletCreateDto createDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(createDto);

        using var response = await _httpClient.PostAsJsonAsync(_basePath, createDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> UpdateAsync(Guid id, WalletUpdateDto updateDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        using var response = await _httpClient.PutAsJsonAsync($"{_basePath}/{id}", updateDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync($"{_basePath}/{id}", ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }
}
