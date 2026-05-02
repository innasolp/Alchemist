using Alchemist.Common;
using Alchemist.DependencyInjection.Common;
using Microsoft.Extensions.Configuration;

namespace Alchemist.Test.Server.Fixtures;

public static class ConfigurationHostExtensions
{
    public static void SetKestrelLocalhostPortsConfig(this IConfiguration configuration, int httpPort, int httpsPort)
    {
        configuration.SetLocalhostPortsConfig("Kestrel:EndPoints:Http:Url", httpPort, "Kestrel:EndPoints:Https:Url", httpsPort);
    }

    public static void SetLocalhostPortsConfig(this IConfiguration configuration, string httpPortSection, int httpPort, string httpsPortSection, int httpsPort)
    {
        var http = configuration.GetSection(httpPortSection);
        http.Value = $"https://{Utils.GetEnvironmentLocalhost()}:{httpPort}";
        var https = configuration.GetSection(httpsPortSection);
        https.Value = $"https://{Utils.GetEnvironmentLocalhost()}:{httpsPort}";
    }

    public static bool SetPortToHostSection(this IConfiguration configuration, string hostSection, int port)
    {
        var shopApiHost = configuration.GetSection(hostSection).Get<string>();
        if (!Uri.TryCreate(shopApiHost, UriKind.Absolute, out var uri))
            return false;

        UriBuilder uriBuilder = new(uri)
        {
            Port = port
        };

        configuration.GetSection(hostSection).Value = uriBuilder.Uri.ToString();
        return true;
    }
}