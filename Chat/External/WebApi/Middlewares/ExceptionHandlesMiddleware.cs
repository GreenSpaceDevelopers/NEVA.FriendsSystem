using Application.Abstractions.Services.Reporting;
using Domain.Exceptions;

namespace WebApi.Middlewares;

public class ExceptionHandlesMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedException ex)
        {
            await SendResponse(context, 401, ex.Message);
        }
        catch (LocalizationException ex)
        {
            logger.LogInformation(ex, "{Message}", ex.Message);
            await SendResponse(context, ex.StatusCode, ex.GetError());
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                throw;
            }
            
            var reporters = context.RequestServices.GetServices<IBugReporter>();
            foreach (var reporter in reporters)
            {
                try
                {
                    await reporter.ReportAsync(ex, context);
                }
                catch (Exception reporterEx)
                {
                    logger.LogError(reporterEx, "Ошибка при отправке отчета об ошибке");
                }
            }

            logger.LogError(ex, "{Message}", ex.Message);
            await SendResponse(context, StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private static async Task SendResponse(HttpContext context, int statusCode, string result)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(result);
    }
}