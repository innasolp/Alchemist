using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Alchemist.WebApp.Api.Common;

public static class WebApplicationExtenions
{
    public static void UseApiSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseSwagger(options =>
        {
            options.SerializeAsV2 = true;
        });
    }

    public static void SetApiRoute<TAppBuilder>(this TAppBuilder app)
        where TAppBuilder : IApplicationBuilder, IEndpointRouteBuilder
    {
        app.UseHsts();

        app.MapControllers();

        app.MapGet("/", () => "Hello ImportSettingsWebApp API!");
    }
}
