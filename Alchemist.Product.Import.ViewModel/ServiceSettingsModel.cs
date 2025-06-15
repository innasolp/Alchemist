using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.Model;

public class ServiceSettingsModel: SettingsModelBase, IImportServiceSettings, IJsonValue, IJsonOnDeserialized, IEquatable<ServiceSettingsModel>
{
    public override string ToString()
    {
        return @$"{nameof(ServiceSettingsModel)}:{nameof(Id)}:{Id};{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(ShopSettingsGuid)}:{ShopSettingsGuid};
            {nameof(ServiceTypeName)}:{ServiceTypeName};
            {nameof(ServiceProviderPath)}:{ServiceProviderPath};
            {nameof(AssemblyPath)}:{AssemblyPath};
            {nameof(ImplementationTypeName)}:{ImplementationTypeName}";
    }

    public Guid ShopSettingsGuid { get; set; }

    public ShopSettingType ShopSettingType => ShopSettingType.Service;
    
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

    [Display(Name = "Value in json format")]
    [Remote(action: "InvalidJsonValue",
        controller: "Validation",
        HttpMethod = "POST",
        ErrorMessage = "Invalid json value")]
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

    public override TabType Tab => TabType.Shop;

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(AssemblyPath) && string.IsNullOrEmpty(ServiceProviderPath)
            && string.IsNullOrEmpty(ServiceTypeName) && string.IsNullOrEmpty(ImplementationTypeName);
    }

    public override void Update(SettingsModelBase source)
    {
        if(source is ServiceSettingsModel serviceSettingsModel)
            Update(serviceSettingsModel);
        else 
            base.Update(source);
    }

    public void Update(ServiceSettingsModel source)
    {
        if(!ModelHelper.IsServiceSettingsPrimary(Name))
            base.Update(source);
        ServiceTypeName = source.ServiceTypeName;
        ImplementationTypeName = source.ImplementationTypeName;
        AssemblyPath = source.AssemblyPath;
        ServiceProviderPath = source.ServiceProviderPath;
        Value = source.Value ?? (!string.IsNullOrEmpty(source.StringValue) ? JsonSerializer.Deserialize<JsonObject>(source.StringValue) : null);
        StringValue = source.StringValue;
        FileName = source.FileName;
    }

    void IJsonOnDeserialized.OnDeserialized()
    {
        this.DeserializeValueIfNeed();
        if(Value != null)
            StringValue = Value.ToJsonString();
    }

    public bool Equals(ServiceSettingsModel? other)
    {
        return other != null && Name == other.Name
           && ((string.IsNullOrEmpty(ServiceTypeName) && string.IsNullOrEmpty(other.ServiceTypeName))
             || string.Equals(ServiceTypeName, other.ServiceTypeName, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(AssemblyPath) && string.IsNullOrEmpty(other.AssemblyPath))
             || string.Equals(AssemblyPath, other.AssemblyPath, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(ServiceProviderPath) && string.IsNullOrEmpty(other.ServiceProviderPath))
             || string.Equals(ServiceProviderPath, other.ServiceProviderPath, StringComparison.CurrentCultureIgnoreCase))
             && ((string.IsNullOrEmpty(ImplementationTypeName) && string.IsNullOrEmpty(other.ImplementationTypeName))
             || string.Equals(ImplementationTypeName, other.ImplementationTypeName, StringComparison.CurrentCultureIgnoreCase))
             && ((Value == null && other.Value == null) || JsonNode.DeepEquals(Value, other.Value));
    }
}
