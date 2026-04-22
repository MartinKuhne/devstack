using DevStack.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DevStack.Api.Middlewares;

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
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        
        var problemDetails = exception switch
        {
            NotFoundException notFound => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Type = "https://devstack.io/errors/not-found",
                Title = "Resource not found",
                Detail = notFound.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["errorCode"] = "NOT_FOUND",
                    ["entityType"] = notFound.Type,
                    ["entityKey"] = notFound.Key.ToString()
                }
            },
            ConcurrencyException concurrency => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Type = "https://devstack.io/errors/concurrency",
                Title = "Concurrency conflict",
                Detail = concurrency.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["errorCode"] = "CONCURRENCY_CONFLICT"
                }
            },
            ValidationException validation => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Type = "https://devstack.io/errors/validation",
                Title = "Validation failed",
                Detail = validation.Message,
                Instance = context.Request.Path,
                Extensions =
                {
                    ["errorCode"] = "VALIDATION_ERROR"
                }
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "https://devstack.io/errors/server-error",
                Title = "An error occurred",
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["errorCode"] = "SERVER_ERROR"
                }
            }
        };

        context.Response.StatusCode = problemDetails.Status!.Value;
        
        var serializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(problemDetails, serializerOptions);
        return context.Response.WriteAsync(json);
    }
}
