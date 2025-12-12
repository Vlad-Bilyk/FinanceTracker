using System.Net.Http.Headers;

namespace FinanceTracker.BlazorUI.Services.Auth;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly AuthState _authState;

    public AuthMessageHandler(AuthState authState)
    {
        _authState = authState ?? throw new ArgumentNullException(nameof(authState));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _authState.EnsureInitializedAsync();

        if (_authState.IsAuthenticated &&
            !request.Headers.Contains("Authorization"))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.JwtToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
