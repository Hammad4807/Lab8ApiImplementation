using System.Diagnostics;

namespace Lab8ApiImplementation.Middlewares
{
    // Logs HTTP Method, Request Path and how long the request took (ms)
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Start the timer
            var stopwatch = Stopwatch.StartNew();

            // Pass the request to the next middleware
            await _next(context);

            // Response came back, stop the timer
            stopwatch.Stop();

            _logger.LogInformation("HTTP {Method} {Path} took {ElapsedMs} ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
