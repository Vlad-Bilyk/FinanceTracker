using FinanceTracker.BlazorUI.Models.Commons;

namespace FinanceTracker.BlazorUI.Services.Commons;

/// <summary>
/// Provides helper methods for converting HTTP responses into ApiResult objects.
/// </summary>
public interface IApiErrorHandler
{
    /// <summary>
    /// Creates an <see cref="ApiResult"/> instance based on the HTTP response content.
    /// </summary>
    /// <param name="response">HTTP response message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed <see cref="ApiResult"/>.</returns>
    Task<ApiResult> CreateResultAsync(HttpResponseMessage response, CancellationToken cancellationToken = default);
}
