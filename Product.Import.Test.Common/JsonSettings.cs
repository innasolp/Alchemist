using Json.CustomSerialization;

namespace Product.Import.Json.Test.Common;

public class JsonSettings() : IJsonSettings
{
    public Dictionary<string, string[]> PropertyNodePathes { get; set; }

    public Dictionary<string, JsonSettings> ItemJsonSettings { get; set; }

    Dictionary<string, IJsonSettings> IJsonSettings.ItemJsonSettings => ItemJsonSettings.ToDictionary(i=>i.Key, i=>i.Value as IJsonSettings);
}