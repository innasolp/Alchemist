using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using DependencyInjection.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public class ServiceSettingsModel: IImportServiceSettings
{
    public Guid Guid { get; set; } = Guid.NewGuid();

    public int? Id { get; set; }

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

    [JsonPropertyName("Value")]
    public JsonObject? JsonValue { get; set; }

    public int? ParentSettingsId { get; set; }

    public string? FileName { get; set; }
   
    public string? StringValue { get; set; }

    string? IServiceSettings.Value { get => StringValue; set => StringValue = value; }

    public void Update(ServiceSettingsModel source)
    {
        ShopSettingType = source.ShopSettingType;
        ServiceTypeName = source.ServiceTypeName;
        ImplementationTypeName = source.ImplementationTypeName;
        AssemblyPath = source.AssemblyPath;
        ServiceProviderPath = source.ServiceProviderPath;
        JsonValue = source.JsonValue;
        StringValue = source.StringValue;
        FileName = source.FileName;
    }
}
