using System.Diagnostics;

namespace HRMS.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();
            _logger.LogInformation("{Method} {Path} -> {Status} in {Ms}ms | {User}",
                context.Request.Method, context.Request.Path, context.Response.StatusCode,
                sw.ElapsedMilliseconds, context.User.Identity?.Name ?? "anonymous");
        }
        catch { sw.Stop(); throw; }
    }
}
