using FinanceTracker.BlazorUI.Models.Auth;
using FinanceTracker.BlazorUI.Models.Commons;
using FinanceTracker.BlazorUI.Services.Commons;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.ApiClients;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiErrorHandler _apiErrorHandler;

    public AuthApiClient(HttpClient httpClient, IApiErrorHandler apiErrorHandler)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiErrorHandler = apiErrorHandler ?? throw new ArgumentNullException(nameof(apiErrorHandler));
    }
    
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, ct);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(ct);
        return authResponse;
    }

    public async Task<ApiResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, ct);

        return await _apiErrorHandler.CreateResultAsync(response, ct);
    }
}
