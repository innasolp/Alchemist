using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.Import.WebApp.Models;

public class ServiceSettingsModel
{
    public Guid Guid { get; set; } = Guid.NewGuid();

    public int Id { get; set; }

    public int ShopId { get; set; }

    public int ShopSettingsId { get; set; }
    
    public ShopSettingType ShopSettingType { get; set; }    

    //todo must be required, but is optional for deserializing
    public string ServiceName { get; set; }

    [Required]
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

    [Display(Name = "All data in json")]
    [Required(AllowEmptyStrings = true)]
    [Remote(action: "AssemblyPathForJsonValueNotEmpty",
        controller: "Validation",
        HttpMethod = "POST",
        AdditionalFields = $"{nameof(AssemblyPath)}",
        ErrorMessage = "Assembly path for is required for loading value from json.")]
    public string? JsonValue { get; set; }

    public virtual void Update(ServiceSettingsModel source)
    {
        ShopSettingType = source.ShopSettingType;
        ServiceTypeName = source.ServiceTypeName;
        ImplementationTypeName = source.ImplementationTypeName;
        AssemblyPath = source.AssemblyPath;
        ServiceProviderPath = source.ServiceProviderPath;
        JsonValue = source.JsonValue;
    }
}
