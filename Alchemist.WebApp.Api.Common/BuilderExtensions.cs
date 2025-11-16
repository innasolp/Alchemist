using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.WebApp.Api.Common;

public static class BuilderExtensions
{
    public static void AddSwaggerApi(this IServiceCollection services)
    {
        services.AddAuthentication("https");
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public static bool IsApi(this IHostApplicationBuilder builder, string[] args)
    {
        return args.Length > 0 &&
            args.Contains("-api", StringComparer.InvariantCultureIgnoreCase)
            || args.Contains("--api=true", StringComparer.InvariantCultureIgnoreCase)
            || builder.Configuration.GetValue<bool>("isApi");
    }
}
