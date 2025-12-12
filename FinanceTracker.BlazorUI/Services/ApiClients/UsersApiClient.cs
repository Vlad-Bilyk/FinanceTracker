using FinanceTracker.BlazorUI.Models.Commons;
using FinanceTracker.BlazorUI.Models.Operation;
using FinanceTracker.BlazorUI.Models.User;
using FinanceTracker.BlazorUI.Services.Commons;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class UsersApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiErrorHandler _apiErrorHandler;
    private const string _basePath = "api/users";

    public UsersApiClient(HttpClient httpClient, IApiErrorHandler apiErrorHandler)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiErrorHandler = apiErrorHandler ?? throw new ArgumentNullException(nameof(apiErrorHandler));
    }

    public async Task<UserDto?> GetMe(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<UserDto>($"{_basePath}/me", ct);
        return result;
    }

    public async Task<ApiResult> UpdateAsync(
        Guid id, UserUpdateDto updateDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        using var response = await _httpClient.PutAsJsonAsync(
            $"{_basePath}/{id}", updateDto, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult>  UpdatePasswordAsync(
        ChangePasswordRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _httpClient.PutAsJsonAsync(
            $"{_basePath}/me/change-password", request, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }

    public async Task<ApiResult> DeleteAsync(
        Guid id, CancellationToken ct = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"{_basePath}/{id}", ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }
}
