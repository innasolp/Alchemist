using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Alchemist.Common;

public static class ConfigurationExtensions
{
    public static async Task<T?> GetJsonSettings<T>(this string jsonFile)
    {
        var s = File.OpenRead(jsonFile);

        var settings = await JsonSerializer.DeserializeAsync<T>(s);

        s.Close();

        return await Task.FromResult(settings);
    }

    public static T? GetSettings<T>(this string jsonFile, string sectionName)
    {
        var confBuilder = new ConfigurationBuilder().AddJsonFile(jsonFile, optional: true, reloadOnChange: true);//.AddEnvironmentVariables();
        var conf = confBuilder.Build();
        return conf.GetSection(sectionName).Get<T>();
    }
}
