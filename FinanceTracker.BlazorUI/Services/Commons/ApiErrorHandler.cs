using FinanceTracker.BlazorUI.Models.Commons;
using System.Net.Http.Json;

namespace FinanceTracker.BlazorUI.Services.Commons;

/// <summary>
/// Default implementation of <see cref="IApiErrorHandler"/> that parses ProblemDetails responses.
/// </summary>
public class ApiErrorHandler : IApiErrorHandler
{
    /// <inheritdoc/>
    public async Task<ApiResult> CreateResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return ApiResult.Success();
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(cancellationToken);

        if (!string.IsNullOrWhiteSpace(problem?.Detail))
        {
            return ApiResult.Failure([problem.Detail]);
        }

        return ApiResult.Failure(["An unexpected error occurred while processing the request."]);
    }
}
