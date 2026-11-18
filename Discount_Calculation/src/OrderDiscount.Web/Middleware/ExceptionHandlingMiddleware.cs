using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrderDiscount.Domain.Exceptions;
using OrderDiscount.Web.Resources;

namespace OrderDiscount.Web.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IStringLocalizer<SharedResource> localizer)
    {
        try { await next(context); }
        catch (DomainValidationException ex)
        {
            var loc = localizer[ex.ErrorCode, ex.Args];
            var msg = loc.ResourceNotFound ? ex.Message : loc.Value;
            logger.LogWarning("Validation failed ({ErrorCode}): {Message}", ex.ErrorCode, msg);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = 400, Title = "Validation Error", Detail = msg, Extensions = { ["errorCode"] = ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = 500, Title = "Server Error", Detail = "An unexpected error occurred." });
        }
    }
}