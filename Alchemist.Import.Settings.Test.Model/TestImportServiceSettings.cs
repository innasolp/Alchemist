using Alchemist.Import.Settings.Extensions;
using Import.Settings.Interfaces;
using ShopSettings.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Test.Model;

public class TestImportServiceSettings : IServiceSettings, IShopSettings, IJsonOnDeserialized, IJsonNodeValue
{   
    public string Name { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? AssemblyPath { get; set; }
    public string? ServiceProviderPath { get; set; }
    public string? ImplementationTypeName { get; set; }
    public int? Id { get; set; }

    [JsonIgnore]
    public JsonNode? Value { get; set; }

    [JsonPropertyName("Value")]
    public JsonElement? ValueObj { get; set; }

    public int? ParentSettingsId { get; set; }
    public int ShopId { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();    

    string? IServiceSettings.Value { 
        get => Value?.ToString();
        set {
            Value = value != null ? JsonSerializer.Deserialize<JsonObject>(value) : null;
        } 
    }
    int IShopSettings.Id { get =>Id ?? 0; set => Id = value; }

    ShopSettingType IShopSettings.Type
    {
        get { return ShopSettingType.Service; }
        set {; }
    }

    bool? IShopSettings.IsActual { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    string IShopSettings.JsonValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    
    void IJsonOnDeserialized.OnDeserialized()
    {
        this.DeserializeValueIfNeed();
    }

}
