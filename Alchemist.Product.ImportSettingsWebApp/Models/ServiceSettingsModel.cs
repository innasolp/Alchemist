using Alchemist.Common;
using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class ServiceSettingsModel : IServiceSettings, IShopSettings
{    
    public Guid Guid { get; set; } = Guid.NewGuid();

    [Required(AllowEmptyStrings = true)]
    [Display(Name = "Service type")]
    [Remote(action: nameof(ImportSettingsValidationController.AssemblyPathOrProviderPathNotEmpty),
        controller: "ImportSettingsValidation",
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
    [Display(Name = "Value in json format")]
    [Remote(action: nameof(ImportSettingsValidationController.InvalidJsonValue),
        controller: "ImportSettingsValidation",
        HttpMethod = "POST",
        ErrorMessage = "Invalid json value")]
    public string? Value { get; set; }

    [JsonPropertyName("Value")]
    [JsonConverter(typeof(JsonValueConverter))]
    public JsonObject? JsonValue { get; set; }

    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public int? ParentSettingsId { get; set; }

    public int ShopId { get; set; }

    public string? FileName { get; set; }

    [JsonIgnore]
    public ShopSettingType ShopSettingType => ShopSettingType.Service;

    bool? IShopSettings.IsActual { get; set; }

    [JsonIgnore]
    string IShopSettings.JsonValue { get => Value; set => Value  = value; }

    [JsonIgnore]
    ShopSettingType IShopSettings.Type { get => ShopSettingType; set {; }  }
}