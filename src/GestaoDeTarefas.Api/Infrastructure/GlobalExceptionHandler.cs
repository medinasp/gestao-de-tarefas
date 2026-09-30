using FluentValidation;
using GestaoDeTarefas.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeTarefas.Api.Infrastructure;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = Map(exception);
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        if (problemDetails.Status >= 500)
        {
            logger.LogError(exception, "Unhandled exception.");
        }
        else
        {
            logger.LogWarning("Request failed: {Message}", exception.Message);
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ProblemDetails Map(Exception exception) => exception switch
    {
        ValidationException validation => BuildValidationProblem(validation),
        NotFoundException notFound => new ProblemDetails
        {
            Title = "Resource not found",
            Status = StatusCodes.Status404NotFound,
            Detail = notFound.Message
        },
        ArgumentException argument => new ProblemDetails
        {
            Title = "Invalid request",
            Status = StatusCodes.Status400BadRequest,
            Detail = argument.Message
        },
        _ => new ProblemDetails
        {
            Title = "An unexpected error occurred",
            Status = StatusCodes.Status500InternalServerError
        }
    };

    private static ValidationProblemDetails BuildValidationProblem(ValidationException validation)
    {
        var errors = validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest
        };
    }
}
