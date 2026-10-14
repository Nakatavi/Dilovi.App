using System.Net;
using Application.Enums;
using Application.Features.Responses;
using Domain.Common;
using FluentValidation;

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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors.Select(e => e.ErrorMessage));

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new ErrorResponse(ErrorMessageLevel.Error, ex.Errors));
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
        DomainException => HttpStatusCode.UnprocessableEntity,
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
