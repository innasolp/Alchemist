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
}
