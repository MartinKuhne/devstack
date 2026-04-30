using System.Net;

using DevStack.Domain.Exceptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevStack.Infrastructure.Mapping;

public static class ExceptionMapper
{
    public static ProblemDetails ToProblemDetails(Exception exception, HttpContext? httpContext = null)
    {
        var requestPath = httpContext?.Request.Path.ToString() ?? "";

        return exception switch
        {
            NotFoundException notFound => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Type = "https://devstack.io/errors/not-found",
                Title = "Resource not found",
                Detail = notFound.Message,
                Instance = requestPath,
                Extensions =
                {
                    ["errorCode"] = "NOT_FOUND",
                    ["entityType"] = notFound.Type,
                    ["entityKey"] = notFound.Key.ToString()!
                }
            },
            ConcurrencyException concurrency => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Type = "https://devstack.io/errors/concurrency",
                Title = "Concurrency conflict",
                Detail = concurrency.Message,
                Instance = requestPath,
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
                Instance = requestPath,
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
                Instance = requestPath,
                Extensions =
                {
                    ["errorCode"] = "SERVER_ERROR"
                }
            }
        };
    }
}
