using System.Collections.Concurrent;

namespace AutoStock.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);
    private readonly int _authLimit = 20;
    private readonly int _generalLimit = 100;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString();
        var isAuth = path.StartsWith("/api/auth");
        var limit = isAuth ? _authLimit : _generalLimit;

        var entry = _entries.GetOrAdd(ip, _ => new RateLimitEntry());

        lock (entry)
        {
            if (DateTime.UtcNow - entry.WindowStart > _window)
            {
                entry.WindowStart = DateTime.UtcNow;
                entry.Count = 0;
            }

            entry.Count++;

            if (entry.Count > limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("Retry-After", "60");
                return;
            }
        }

        await _next(context);
    }

    private class RateLimitEntry
    {
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
        public int Count { get; set; }
    }
}
