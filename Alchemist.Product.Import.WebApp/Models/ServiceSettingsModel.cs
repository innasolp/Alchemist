using Alchemist.Import.Settings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.Model;
using DependencyInjection.Interfaces;

namespace Alchemist.Product.Import.WebApp.Models;

public class ServiceSettingsModel : SettingsModelBase, IServiceSettingsModel
{
    public int Id { get; set; }

    public override string ToString()
    {
        return @$"{nameof(ServiceSettingsModel)}:{nameof(Id)}:{Id};{nameof(Name)}:{Name};{nameof(Guid)}:{Guid};{nameof(ShopGuid)}:{ShopGuid};{nameof(ShopSettingsGuid)}:{ShopSettingsGuid};
            {nameof(ServiceTypeName)}:{ServiceTypeName};
            {nameof(ServiceProviderPath)}:{ServiceProviderPath};
            {nameof(AssemblyPath)}:{AssemblyPath};
            {nameof(ImplementationTypeName)}:{ImplementationTypeName}";
    }

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

    public override TabType Tab => TabType.Shop;

    public int? ParentSettingsId { get; set; }

    public Guid ShopSettingsGuid { get; }
    
    string? IServiceSettings.Value { get => StringValue; set => StringValue = value; }

    int ISettings.ShopId { get => ShopId; set { } }

    public ServiceSettingsModel() : base(0, Guid.Empty) { }

    public ServiceSettingsModel(int shopId, int id, int parentId, Guid shopSettingsGuid, Guid shopGuid, string serviceName) : base(shopId, shopGuid)
    {
        Id = id;
        ParentSettingsId = parentId;
        ShopSettingsGuid = shopSettingsGuid;
        Name = serviceName;
    }

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
}
