using Http.ErrorHandling;
using Http.Info;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.WebApp.Api.Common;

public static class HttpInterceptionExtensions
{
    public static void AddBaseControllerInterceptors<TController>(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler<TController>>();
        services.AddSingleton<InfoLogMiddleware<TController>>();
        services.AddProblemDetails();
    }

    public static void UseBaseInterceptors<TController>(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseMiddleware<InfoLogMiddleware<TController>>();
    }
}
