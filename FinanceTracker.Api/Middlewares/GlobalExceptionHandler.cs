using FinanceTracker.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Middlewares;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService ?? throw new ArgumentNullException(nameof(problemDetailsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Server error | Type: {ExceptionType} | " +
                "Path: {Method} {Path} | Message: {Message} | Source: {Source}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message,
                exception.Source);

            if (exception.InnerException != null)
            {
                _logger.LogError(
                    exception.InnerException,
                    "Inner exception: {InnerExceptionType}",
                    exception.InnerException.GetType().Name);
            }
        }
        else
        {
            _logger.LogDebug(
                exception,
                "Client error | Type: {ExceptionType} | Status: {StatusCode}",
                exception.GetType().Name,
                statusCode);
        }

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Type = exception.GetType().Name,
                Title = "An error occured",
                Detail = exception.Message
            }
        };

        var wrote = await _problemDetailsService.TryWriteAsync(context);

        if (!wrote && !httpContext.Response.HasStarted)
        {
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(context.ProblemDetails, cancellationToken);
        }

        return true;
    }
}
