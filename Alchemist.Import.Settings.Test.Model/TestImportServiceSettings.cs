using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Test.Model;

public class TestImportServiceSettings : IImportServiceSettings, IJsonOnDeserialized, IJsonValue
{   
    public string? Name { get; set; }
    public string? ServiceTypeName { get; set; }
    public string? AssemblyPath { get; set; }
    public string? ServiceProviderPath { get; set; }
    public string? ImplementationTypeName { get; set; }
    public int? Id { get; set; }

    [JsonIgnore]
    public JsonObject? Value { get; set; }

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
    int ISettings.Id { get =>Id ?? 0; set => Id = value; }

    ShopSettingType ISettings.ShopSettingType => ShopSettingType.Service;

    void IJsonOnDeserialized.OnDeserialized()
    {
        this.DeserializeValueIfNeed();
    }
}
