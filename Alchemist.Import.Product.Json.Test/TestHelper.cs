using Alchemist.Common;
using Json.CustomSerialization;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Product.Json.Test;

internal static class TestHelper
{
    internal static T GetItemFromJson<T>(string dataJsonFile, string settingsJsonFile)
        where T:IJsonItem , new()
    {        
        var jsonSettings = settingsJsonFile.ReadFromJsonFile<JsonSettings>();

        return GetItemFromJson<T>(dataJsonFile, jsonSettings);
    }

    internal static T GetItemFromJson<T>(string dataJsonFile, IJsonSettings jsonSettings)
        where T:IJsonItem , new()
    {        
        var json = dataJsonFile.ReadFromJsonFile<JsonObject>();
        Assert.NotNull(json);

        var jsonLoader = new JsonLoader(jsonSettings);
        var item = new T();
        jsonLoader.LoadFromJson(item, json);

        return item;
    }
}