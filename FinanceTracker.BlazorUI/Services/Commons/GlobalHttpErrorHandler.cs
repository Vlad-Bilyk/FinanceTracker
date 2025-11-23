using FinanceTracker.BlazorUI.Services.Auth;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace FinanceTracker.BlazorUI.Services.Commons;

public class GlobalHttpErrorHandler : DelegatingHandler
{
    private readonly NavigationManager _navigationManager;
    private readonly ISnackbar _snackbar;
    private readonly AuthState _authState;

    public GlobalHttpErrorHandler(NavigationManager navigationManager,
        ISnackbar snackbar, AuthState authState)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _snackbar = snackbar ?? throw new ArgumentNullException(nameof(snackbar));
        _authState = authState ?? throw new ArgumentNullException(nameof(authState));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _authState.ClearAsync(cancellationToken);

            _snackbar.Add("Your session has expired. Please login again.", Severity.Warning);
            _navigationManager.NavigateTo("/login");
            return response;
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            // TODO: add navigation to /500 page
            _snackbar.Add("Server error occurred. Please try again later.", Severity.Error);
            return response;
        }

        return response;
    }
}
