using FinanceTracker.BlazorUI.Models.Commons;
using FinanceTracker.BlazorUI.Models.Operation;
using FinanceTracker.BlazorUI.Services.Commons;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class OperationsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiErrorHandler _apiErrorHandler;
    private const string _basePath = "api/wallets";

    public OperationsApiClient(HttpClient httpClient, IApiErrorHandler apiErrorHandler)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiErrorHandler = apiErrorHandler ?? throw new ArgumentNullException(nameof(apiErrorHandler));
    }

    public async Task<IReadOnlyList<OperationDetailsDto>> GetWalletOperationsAsync(
        Guid walletId, CancellationToken ct = default)
    {
        var result = await _httpClient
            .GetFromJsonAsync<IReadOnlyList<OperationDetailsDto>>(
                $"{_basePath}/{walletId}/operations", ct);
        return result ?? [];
    }

    public async Task<PagedResult<OperationDetailsDto>> GetPagedAsync(
        Guid? walletId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = new Dictionary<string, string?>
        {
            ["walletId"] = walletId.ToString(),
            ["from"] = from.ToString(),
            ["to"] = to.ToString(),
            ["page"] = page.ToString(),
            ["pageSize"] = pageSize.ToString()
        };

        var url = QueryHelpers.AddQueryString("api/operations", query);
        var result = await _httpClient.GetFromJsonAsync<PagedResult<OperationDetailsDto>>(url, ct);
        return result ?? new PagedResult<OperationDetailsDto>();
    }

    public async Task<ApiResult> CreateAsync(
        Guid walletId, OperationUpsertDto createDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(createDto);

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_basePath}/{walletId}/operations", createDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> UpdateAsync(
       Guid walletId, Guid id, OperationUpsertDto updateDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        using var response = await _httpClient.PutAsJsonAsync(
            $"{_basePath}/{walletId}/operations/{id}", updateDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> DeleteAsync(
        Guid walletId, Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"{_basePath}/{walletId}/operations/{id}", ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }
}
