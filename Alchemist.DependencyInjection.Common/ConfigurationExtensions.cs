using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Alchemist.Common;

namespace Alchemist.DependencyInjection.Common;

public static class ConfigurationExtensions
{
    public static string? GetHostSectionValue(this IHostApplicationBuilder Builder, string sectionName)
    {
        return Builder.Configuration.GetSection(sectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
    }
}
