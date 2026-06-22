using System.Text.Json;

namespace TabibLens.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, exception.Message),
                InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
                JsonException => (StatusCodes.Status400BadRequest, "Invalid data format."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred");
            }
            else
            {
                _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new { message };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
