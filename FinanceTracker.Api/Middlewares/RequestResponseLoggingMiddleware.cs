using FinanceTracker.Application.Interfaces.Common;
using Serilog.Context;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FinanceTracker.Api.Middlewares;

internal sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private static readonly string[] _sensitiveFields = ["password", "jwtToken", "apiKey"];

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var correlationId = GetOrCreateCorrelationId(context);
        var userId = GetUserId(userContext);
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId))
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["UserId"] = userId
        }))
        {
            await LogRequestAsync(context);

            var originalResponseBody = context.Response.Body;

            using var responseMemoryStream = new MemoryStream();
            context.Response.Body = responseMemoryStream;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                responseMemoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await ReadStreamAsync(responseMemoryStream);

                LogResponseAsync(context, stopwatch.ElapsedMilliseconds, responseBody);

                responseMemoryStream.Seek(0, SeekOrigin.Begin);
                await responseMemoryStream.CopyToAsync(originalResponseBody);

                context.Response.Body = originalResponseBody;
            }
        }
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        var request = context.Request;

        var requestBody = await ReadRequestBodyAsync(context.Request);
        var sanitizedBody = RedactSensitiveData(requestBody);

        _logger.LogInformation(
           "HTTP Request: {Method} {Path} {QueryString} | " +
           "ContentType: {ContentType} | " +
           "Body: {RequestBody}",
           request.Method,
           request.Path.Value,
           request.QueryString.HasValue ? request.QueryString.Value : "",
           request.ContentType,
           sanitizedBody);
    }

    private void LogResponseAsync(HttpContext context, long elapsedMs, string responseBody)
    {
        var response = context.Response;
        var sanitizedBody = RedactSensitiveData(responseBody);

        LogLevel logLevel = response.StatusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(
            logLevel,
            "HTTP Response: {StatusCode} | " +
            "ElapsedMs: {ElapsedMs} | " +
            "Body: {ResponseBody}",
            response.StatusCode,
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

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var content = await reader.ReadToEndAsync();
        stream.Seek(0, SeekOrigin.Begin);

        return content;
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

    private static string GetUserId(IUserContext userContext)
    {
        return userContext.UserId?.ToString() ?? "Anonymous";
    }

    private static bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        return path.StartsWith("/swagger");
    }
}
