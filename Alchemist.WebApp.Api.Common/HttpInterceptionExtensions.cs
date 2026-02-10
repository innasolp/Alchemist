using Http.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.WebApp.Api.Common;

public static class HttpInterceptionExtensions
{
    public static void AddBaseControllerInterceptors(this IServiceCollection services)
    {
        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.RequestBody
            | HttpLoggingFields.RequestPath
            | HttpLoggingFields.ResponseBody
            | HttpLoggingFields.RequestHeaders
            | HttpLoggingFields.ResponseHeaders;
            logging.RequestBodyLogLimit = 4096; // Set limits to prevent performance issues
            logging.ResponseBodyLogLimit = 4096;
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }

    public static void UseBaseInterceptors(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseHttpLogging();
    }
}
