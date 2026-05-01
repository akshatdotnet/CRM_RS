using System.Net;
using System.Text.Json;
using HRMS.Application.Common;

namespace HRMS.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, message) = ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("{Type}: {Msg}", ex.GetType().Name, ex.Message);
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(ApiResponse.Fail(message),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
