using FinanceTracker.BlazorUI.Models.Auth;
using FinanceTracker.BlazorUI.Models.Common;
using System.Net;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.Auth;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, ct);
        if (!response.IsSuccessStatusCode)
        {
            // TODO: Add error processing
            return null;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>(ct);
        return authResponse;
    }

    public async Task<ApiResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var response = await _httpClient.PostAsJsonAsync("api/auth/register", request, ct);

        if (response.IsSuccessStatusCode)
        {
            return ApiResult.Success();
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(ct);

        if (problem?.Errors?.Count > 0)
        {
            var fieldErrors = new Dictionary<string, string[]>();
                
            foreach(var kvp in problem.Errors)
            {
                fieldErrors[kvp.Key] = kvp.Value;
            }

            return ApiResult.Failure(null, fieldErrors);
        }

        if (!string.IsNullOrWhiteSpace(problem?.Detail))
        {
            return ApiResult.Failure([problem.Detail]);
        }

        return ApiResult.Failure(["Registration failed. Please try again later."]);
    }
}
