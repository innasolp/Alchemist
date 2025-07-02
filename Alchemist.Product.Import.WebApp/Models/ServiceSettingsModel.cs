using Alchemist.Import.Settings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.Model;
using DependencyInjection.Interfaces;

namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public class ServiceSettingsModel(int shopId, int id, int parentSettingsId, Guid shopSettingsGuid, Guid shopGuid) 
    : SettingsModelBase(shopId, shopGuid), IServiceSettingsModel
{ 
    string? ISettings.Name { get => Name; set=> Name = value; }

    public int Id { get; set; } = id;

    public override string ToString()
    {
        return @$"{nameof(ServiceSettingsModel)}:{nameof(Id)}:{Id};{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(ShopSettingsGuid)}:{ShopSettingsGuid};
            {nameof(ServiceTypeName)}:{ServiceTypeName};
            {nameof(ServiceProviderPath)}:{ServiceProviderPath};
            {nameof(AssemblyPath)}:{AssemblyPath};
            {nameof(ImplementationTypeName)}:{ImplementationTypeName}";
    }

    [JsonIgnore]
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
    public JsonObject? JsonValue { get; set; }

    public string? FileName { get; set; }

    [Display(Name = "Value in json format")]
    [Remote(action: "InvalidJsonValue",
        controller: "Validation",
        HttpMethod = "POST",
        ErrorMessage = "Invalid json value")]
    public string? StringValue { get; set; }

    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    [JsonInclude]
    public int ParentSettingsId { get; set; } = parentSettingsId;

    int? ISettings.ParentSettingsId { get => ParentSettingsId; set => ParentSettingsId = value ?? 0; }


    [JsonInclude]
    public Guid ShopSettingsGuid { get; private set; } = shopSettingsGuid;

    string? IServiceSettings.Value { get => StringValue; set => StringValue = value; }

}
