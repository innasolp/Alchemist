using Microsoft.AspNetCore.SignalR;

namespace Alchemist.Product.SignalR;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication("https");
        services.AddSignalR(options => options.AddFilter<LogHubFilter>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseAuthorization();
        
        app.UseEndpoints(erb =>
            {
                erb.MapGet("/", () => "Hello signalR!");
                erb.MapHub<EventHub>("/events");
                erb.MapHub<ImportHub>("/import");
            }
        );
       
        app.UseAuthentication();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseHttpsRedirection();
    }
}
