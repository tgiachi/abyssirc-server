using System.Text;
using System.Text.RegularExpressions;

namespace AbyssIrc.Server.Middleware;

public class RestAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RestAuditMiddleware> _logger;

    public RestAuditMiddleware(RequestDelegate next, ILogger<RestAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Save the start time to calculate duration
        var startTime = DateTime.UtcNow;

        // Enable buffering of the request body so it can be read
        context.Request.EnableBuffering();

        // Prepare to read the response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // Log request details before proceeding
            await LogRequest(context.Request);

            // Pass to the next middleware in the pipeline
            await _next(context);

            // Log the response after it has been generated
            await LogResponse(context.Response, startTime);
        }
        catch (Exception ex)
        {
            // Log any exceptions
            _logger.LogError(ex, "An exception occurred during REST request processing");
            throw; // Re-throw the exception to be handled at a higher level
        }
        finally
        {
            // Restore the original response stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);

        string requestBody = string.Empty;
        using (var reader = new StreamReader(
                   request.Body,
                   encoding: Encoding.UTF8,
                   detectEncodingFromByteOrderMarks: false,
                   leaveOpen: true
               ))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        // Reset the pointer to the beginning for subsequent middleware
        request.Body.Seek(0, SeekOrigin.Begin);

        // Filter or mask sensitive data if necessary
        requestBody = MaskSensitiveData(requestBody);

        _logger.LogTrace(
            "REST Request: {Method} {Path} {QueryString} {RequestBody}",
            request.Method,
            request.Path,
            request.QueryString,
            requestBody
        );
    }

    private async Task LogResponse(HttpResponse response, DateTime requestStartTime)
    {
        response.Body.Seek(0, SeekOrigin.Begin);

        string responseBody = string.Empty;
        using (var reader = new StreamReader(
                   response.Body,
                   encoding: Encoding.UTF8,
                   detectEncodingFromByteOrderMarks: false,
                   leaveOpen: true
               ))
        {
            responseBody = await reader.ReadToEndAsync();
        }

        response.Body.Seek(0, SeekOrigin.Begin);

        var duration = DateTime.UtcNow - requestStartTime;

        // Filter or mask sensitive data if necessary
        responseBody = MaskSensitiveData(responseBody);

        _logger.LogTrace(
            "REST Response: StatusCode={StatusCode}, Duration={Duration}ms, Body={ResponseBody}",
            response.StatusCode,
            duration.TotalMilliseconds,
            responseBody
        );

        _logger.LogInformation(
            "{UrlRequest} {Mode} {StatusCode} {Duration}ms",
            response.HttpContext.Request.Path,
            response.HttpContext.Request.Method,
            response.StatusCode,
            duration.TotalMilliseconds
        );
    }

    private static string MaskSensitiveData(string content)
    {
        // Implement logic to mask sensitive data
        // E.g., passwords, tokens, personal information, etc.
        // This is a simple example
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        // You can use regular expressions to mask specific patterns
        // Example: mask passwords in JSON
        content = Regex.Replace(
            content,
            "\"password\"\\s*:\\s*\"[^\"]*\"",
            "\"password\":\"*****\""
        );

        return content;
    }
}

// Extension class for simpler integration
public static class RestAuditMiddlewareExtensions
{
    public static IApplicationBuilder UseRestAudit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RestAuditMiddleware>();
    }
}
