using MaitreKiosk.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MaitreKiosk.Api.Middleware;

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
        catch (AppException ex)
        {
            await WriteProblem(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gözlənilməz xəta");
            await WriteProblem(context, 500, "Daxili server xətası.");
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Detail = detail
        };
        await context.Response.WriteAsJsonAsync(problem);
    }
}