using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Common;

namespace Alchemist.Test.Server.Fixtures;

public static class HostBuilderExtensions
{
    public static IHost CreateTestHostUseAddressConfiguration(this IHostBuilder builder, Action<IWebHostBuilder> configureWebHostBuilder, out IHost host)
    {
        // Create the host for TestServer now before we  
        // modify the builder to use Kestrel instead.    
        var testHost = builder.Build();

        // Modify the host builder to use Kestrel instead  
        // of TestServer so we can listen on a real address.    

        builder.ConfigureWebHost(webHostBuilder => configureWebHostBuilder(webHostBuilder));

        // Create and start the Kestrel server before the test server,  
        // otherwise due to the way the deferred host builder works    
        // for minimal hosting, the server will not get "initialized    
        // enough" for the address it is listening on to be available.    
        // See https://github.com/dotnet/aspnetcore/issues/33846.    

        host = builder.Build();
        host.Start();

        // Extract the selected dynamic port out of the Kestrel server  
        // and assign it onto the client options for convenience so it    
        // "just works" as otherwise it'll be the default http://localhost    
        // URL, which won't route to the Kestrel-hosted HTTP server.     

        //var server = _host.Services.GetRequiredService<IServer>();
        //var addresses = server.Features.Get<IServerAddressesFeature>();

        //ClientOptions.BaseAddress = addresses!.Addresses
        //    .Select(x => new Uri(x))
        //    .Last();

        // Return the host that uses TestServer, rather than the real one.  
        // Otherwise the internals will complain about the host's server    
        // not being an instance of the concrete type TestServer.    
        // See https://github.com/dotnet/aspnetcore/pull/34702.   

        testHost.Start();
        return testHost;
    }

    public static Uri GetBaseAddress(this IHost host)
    {
        var server = host.Services.GetRequiredService<IServer>();
        return server.GetBaseAddress();
    }

    public static Uri GetBaseAddress(this IServer server)
    {
        var addresses = server.Features.Get<IServerAddressesFeature>();
        return addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();
    }

    public static Uri GetBaseAddress(this IWebHost host)
    {
        var server = host.Services.GetRequiredService<IServer>();
        return server.GetBaseAddress();
    }    

    public static void SetLocalhostPortsConfig(this WebHostBuilderContext context, string httpPortSection, int httpPort, string httpsPortSection, int httpsPort)
    {
        var http = context.Configuration.GetSection(httpPortSection);
        http.Value = $"https://{Utils.GetEnvironmentLocalhost()}:{httpPort}";
        var https = context.Configuration.GetSection(httpsPortSection);
        https.Value = $"https://{Utils.GetEnvironmentLocalhost()}:{httpsPort}";
    }

    public static void SetKestrelLocalhostPortsConfig(this WebHostBuilderContext context, int httpPort, int httpsPort)
    {
        context.SetLocalhostPortsConfig("Kestrel:EndPoints:Http:Url", httpPort, "Kestrel:EndPoints:Https:Url", httpsPort);
    }
}
