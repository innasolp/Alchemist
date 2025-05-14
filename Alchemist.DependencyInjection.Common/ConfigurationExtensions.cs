using Microsoft.Extensions.Configuration;
using Alchemist.Common;

namespace Alchemist.DependencyInjection.Common;

public static class ConfigurationExtensions
{
    public static string? GetHostSectionValue(this IConfiguration configuration, string sectionName)
    {
        return configuration.GetSection(sectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
    }
}
