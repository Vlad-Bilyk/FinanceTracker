using Serilog.Context;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FinanceTracker.Api.Middlewares;

internal sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private static readonly string[] _sensitiveFields = ["password", "token"];

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var correlationId = GetOrCreateCorrelationId(context);
        var userId = GetUserId(context);
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))

        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["UserId"] = userId
        }))
        {
            await LogRequestAsync(context, correlationId);

            var originalResponseBody = context.Response.Body;
            using var responseMemoryStream = new MemoryStream();
            context.Response.Body = responseMemoryStream;

            try
            {
                await _next(context);
                stopwatch.Stop();

                await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);
            }
            finally
            {
                responseMemoryStream.Seek(0, SeekOrigin.Begin);
                await responseMemoryStream.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;

        var requestBody = await ReadRequestBodyAsync(context.Request);
        var sanitizedBody = RedactSensitiveData(requestBody);

        _logger.LogInformation(
           "HTTP Request: {Method} {Path} {QueryString} | " +
           "CorrelationId: {CorrelationId} | " +
           "UserId: {UserId} | " +
           "Body: {RequestBody}",
           request.Method,
           request.Path,
           request.QueryString.HasValue ? request.QueryString.Value : "",
           correlationId,
           GetUserId(context),
           sanitizedBody);
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMs)
    {
        var response = context.Response;

        string responseBody = await ReadResponseBodyAsync(response);
        var sanitizedBody = RedactSensitiveData(responseBody);

        LogLevel logLevel;

        if (response.StatusCode >= 500)
        {
            logLevel = LogLevel.Error;
        }
        else if (response.StatusCode >= 400)
        {
            logLevel = LogLevel.Warning;
        }
        else
        {
            logLevel = LogLevel.Information;
        }

        _logger.Log(
            logLevel,
            "HTTP Response: {StatusCode} | " +
            "CorrelationId: {CorrelationId} | " +
            "ElapsedMs: {ElapsedMs} | " +
            "Body: {ResponseBody}",
            response.StatusCode,
            correlationId,
            elapsedMs,
            sanitizedBody);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.ContentLength.HasValue || request.ContentLength == 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();

        request.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin);

        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(
            response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return body;
    }

    private static string? RedactSensitiveData(string? data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        // Simple redaction for sensitive fields
        var pattern = string.Join("|", _sensitiveFields);
        return Regex.Replace(
            data,
            $@"""({pattern})""\s*:\s*""[^""]*""",
            @"""$1"":""***""",
            RegexOptions.IgnoreCase);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            return correlationId!;
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-Id"] = newCorrelationId;
        return newCorrelationId;
    }

    private static string? GetUserId(HttpContext context)
    {
        return context.User?.FindFirst("sub")?.Value
            ?? context.User?.FindFirst("userId")?.Value
            ?? context.User?.Identity?.Name
            ?? "Anonymous";
    }

    private static bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        return path.StartsWith("/swagger");
    }
}
