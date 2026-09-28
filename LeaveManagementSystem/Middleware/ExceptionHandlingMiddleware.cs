using System.Net;
using System.Text.Json;

namespace LeaveManagementSystem.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Authentication or authorization failed.");

                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.Unauthorized,
                    ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Requested resource was not found.");

                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.NotFound,
                    ex.Message);
            }
                    catch (ConflictException ex)
                    {
                    _logger.LogWarning(ex, "Request conflicts with the current resource state.");

                    await WriteErrorResponseAsync(
                        context,
                        HttpStatusCode.Conflict,
                        ex.Message);
                    }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Business validation failed.");

                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unhandled exception occurred while processing the request.");

                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteErrorResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = (int)statusCode,
                message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}