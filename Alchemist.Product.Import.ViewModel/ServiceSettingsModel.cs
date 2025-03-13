using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using DependencyInjection.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Product.Import.Model;

public class ServiceSettingsModel: IImportServiceSettings, IJsonValue, IJsonOnDeserialized
{
    public override string ToString()
    {
        return @$"{nameof(ServiceSettingsModel)}:{nameof(Id)}:{Id};{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(ShopSettingsGuid)}:{ShopSettingsGuid};
            {nameof(ServiceTypeName)}:{ServiceTypeName};
            {nameof(ServiceProviderPath)}:{ServiceProviderPath};
            {nameof(AssemblyPath)}:{AssemblyPath};
            {nameof(ImplementationTypeName)}:{ImplementationTypeName}";
    }

    public Guid Guid { get; set; } = Guid.NewGuid();

    public int Id { get; set; }

    public Guid ShopGuid { get; set; }

    public Guid ShopSettingsGuid { get; set; }

    public string Name { get; set; }

    public ShopSettingType ShopSettingType { get; set; }
    
    [Required(AllowEmptyStrings = true)]
    [Display(Name = "Service type")]
    [Remote(action: "AssemblyPathOrProviderPathNotEmpty",
        controller: "Validation",
        HttpMethod = "POST",
        AdditionalFields = $"{nameof(AssemblyPath)},{nameof(ServiceProviderPath)}",
        ErrorMessage = "Assembly path for implementation type or implementation factory is required")]

    public string? ServiceTypeName { get; set; }

    [Display(Name = "Service implementation type")]
    public string? ImplementationTypeName { get; set; }

    [Display(Name = "Service assembly path")]    
    public string? AssemblyPath { get; set; }

    [Display(Name = "Service assembly path with implementation factory")]
    public string? ServiceProviderPath { get; set; }

    [JsonIgnore]
    public JsonObject? Value { get; set; }

    public string? FileName { get; set; }
   
    public string? StringValue { get; set; }

    [JsonPropertyName("Value")]
    public JsonElement? ValueObj { get; set; }

    string? IServiceSettings.Value { get => Value?.ToString(); set
        {
            Value = value != null ? JsonSerializer.Deserialize<JsonObject>(value) : null;
        }
    }
    int ISettings.ShopId { get ; set; }
    int? ISettings.ParentSettingsId { get; set; }

    public void Update(ServiceSettingsModel source)
    {
        ShopSettingType = source.ShopSettingType;
        ServiceTypeName = source.ServiceTypeName;
        ImplementationTypeName = source.ImplementationTypeName;
        AssemblyPath = source.AssemblyPath;
        ServiceProviderPath = source.ServiceProviderPath;
        Value = source.Value;
        StringValue = source.StringValue;
        FileName = source.FileName;
    }

    void IJsonOnDeserialized.OnDeserialized()
    {
        this.DeserializeValueIfNeed();
    }
}
