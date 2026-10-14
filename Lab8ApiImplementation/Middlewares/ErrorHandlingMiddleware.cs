using Microsoft.AspNetCore.Mvc;

namespace Lab8ApiImplementation.Middlewares
{
    // Catches any error that was not handled, logs it and returns a 500 ProblemDetails
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass the request to the next middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                // Unique id for this request, so we can find it in the logs
                string traceId = context.TraceIdentifier;

                _logger.LogError(ex, "Error on {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    traceId);

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Something went wrong.",
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                };
                problem.Extensions["traceId"] = traceId;

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
