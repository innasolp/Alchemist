using Json.CustomSerialization;
using System.Text.Json.Nodes;
using Xunit;

namespace Product.Import.Json.Test.Common;

public static class TestHelper
{
    public static T GetItemFromJson<T>(string dataJsonFile, string settingsJsonFile)
        where T:IJsonItem , new()
    {        
        var jsonSettings = settingsJsonFile.ReadFromJsonFile<JsonSettings>();

        return GetItemFromJson<T>(dataJsonFile, jsonSettings);
    }

    public static T GetItemFromJson<T>(string dataJsonFile, IJsonSettings jsonSettings)
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