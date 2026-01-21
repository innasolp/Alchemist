using Microsoft.Extensions.Configuration;

namespace Alchemist.Common;

public static class ConfigurationHelper
{
    public static string GetConnectionString(string connectionStringSection)
    {
        var settings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var alchemyDbConnectionString = settings.GetConnectionString(connectionStringSection);
        return alchemyDbConnectionString ?? "";
    }

    public static string? GetSectionValue(string section)
    {
        var settings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        return settings.GetSection(section).Get<string>();
    }
}
