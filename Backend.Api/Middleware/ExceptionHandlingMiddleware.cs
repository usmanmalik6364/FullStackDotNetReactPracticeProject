using Backend.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Middleware;

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
        catch (DuplicateResourceException ex)
        {
            await HandleDuplicateResourceExceptionAsync(context, ex);
        }
        catch (ConcurrencyConflictException ex)
        {
            await HandleConcurrencyConflictAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }
    private static async Task HandleConcurrencyConflictAsync(
        HttpContext context,
        ConcurrencyConflictException exception)
    {
        context.Response.StatusCode =
            StatusCodes.Status409Conflict;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Concurrency conflict",
            Detail = exception.Message
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
    private static async Task HandleDuplicateResourceExceptionAsync(
        HttpContext context,
        DuplicateResourceException exception)
    {
        context.Response.StatusCode =
            StatusCodes.Status409Conflict;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Resource conflict",
            Detail = exception.Message
        };

        await context.Response.WriteAsJsonAsync(problem);
    }

    private async Task HandleUnexpectedExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred while processing the request.");

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "An internal server error occurred."
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}