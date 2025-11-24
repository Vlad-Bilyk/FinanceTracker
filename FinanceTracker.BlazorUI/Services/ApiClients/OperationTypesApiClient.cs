using FinanceTracker.BlazorUI.Models.Commons;
using FinanceTracker.BlazorUI.Models.OperationType;
using FinanceTracker.BlazorUI.Services.Commons;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class OperationTypesApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiErrorHandler _apiErrorHandler;
    private const string _basePath = "api/users/types";

    public OperationTypesApiClient(HttpClient httpClient, IApiErrorHandler apiErrorHandler)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiErrorHandler = apiErrorHandler ?? throw new ArgumentNullException(nameof(apiErrorHandler));
    }

    public async Task<IReadOnlyList<OperationTypeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<OperationTypeDto>>(_basePath, ct);
        return result ?? [];
    }

    public async Task<ApiResult> CreateAsync(OperationTypeCreateDto createDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(createDto);

        using var response = await _httpClient.PostAsJsonAsync(_basePath, createDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> UpdateAsync(Guid id, OperationTypeUpdateDto updateDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        using var response = await _httpClient.PutAsJsonAsync($"{_basePath}/{id}", updateDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> DeleteAsync(Guid id, Guid? replacementTypeId = default, CancellationToken ct = default)
    {
        var requestUri = $"{_basePath}/{id}";

        if (replacementTypeId.HasValue)
        {
            requestUri = $"{requestUri}?replacementTypeId={replacementTypeId.Value}";
        }

        using var response = await _httpClient.DeleteAsync(requestUri, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }
}
