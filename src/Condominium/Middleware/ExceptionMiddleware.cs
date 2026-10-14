using System.Net;

namespace Condominium.Middleware;

public class ExceptionMiddleware : IMiddleware
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";

    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var statusCode = GetStatusCode(ex);

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
            }
            else
            {
                _logger.LogWarning(ex, "Request failed with {StatusCode}", (int)statusCode);
            }

            await WriteErrorAsync(context, ex, statusCode);
        }
    }

    private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        KeyNotFoundException => HttpStatusCode.NotFound,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        _ => HttpStatusCode.InternalServerError
    };

    private static async Task WriteErrorAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Do not leak internal details of unexpected errors to the client
        var message = statusCode == HttpStatusCode.InternalServerError
            ? UnexpectedErrorMessage
            : exception.Message;

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = (int)statusCode,
            message
        });
    }
}
