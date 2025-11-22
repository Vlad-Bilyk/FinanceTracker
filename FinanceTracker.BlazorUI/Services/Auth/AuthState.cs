using Blazored.LocalStorage;

namespace FinanceTracker.BlazorUI.Services.Auth;

public class AuthState
{
    private const string _tokenStorageKey = "authToken";
    private readonly ILocalStorageService _localStorageService;
    private bool _isInitialized;

    public string? JwtToken { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(JwtToken);


    public AuthState(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService ?? throw new ArgumentNullException(nameof(localStorageService));
    }

    public async Task EnsureInitializedAsync(CancellationToken ct = default)
    {
        if (_isInitialized)
        {
            return;
        }

        JwtToken = await _localStorageService.GetItemAsync<string>(_tokenStorageKey, ct);
        _isInitialized = true;
    }

    public async Task SetTokenAsync(string? token, CancellationToken ct = default)
    {
        JwtToken = token;
        
        if (string.IsNullOrWhiteSpace(token))
        {
            await _localStorageService.RemoveItemAsync(_tokenStorageKey, ct);
        }
        else
        {
            await _localStorageService.SetItemAsync(_tokenStorageKey, token, ct);
        }

        _isInitialized = true;
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        JwtToken = null;
        await _localStorageService.RemoveItemAsync(_tokenStorageKey, ct);
        _isInitialized = true;
    }
}
