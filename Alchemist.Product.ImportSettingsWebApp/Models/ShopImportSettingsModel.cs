using Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public abstract class ShopImportSettingsModel : IShopImportSettings, IShopSettings, IJsonOnDeserialized
{
    public bool? Perfomance { get ; set; }

    public int Id { get; set; }

    public string Name { get; set; }

    public int? ParentSettingsId { get; set; }

    public int ShopId { get; set; }

    [JsonIgnore]
    public string JsonValue { get; set; }    

    public Dictionary<string, ServiceSettingsModel> Services { get; set; } = [];

    public abstract ShopSettingType ShopSettingType { get; }

    [JsonIgnore]
    public ServiceSettingsModel? RequestHeaders => this.GetRequestHeaders<ServiceSettingsModel>();

    [Required]
    [JsonIgnore]
    public ServiceSettingsModel ImportService => this.GetImportService<ServiceSettingsModel>();

    [JsonIgnore]
    public ServiceSettingsModel? BrowserDataLoader => this.GetBrowserDataLoader<ServiceSettingsModel>();

    [JsonIgnore]
    public ServiceSettingsModel? BrowserLauncher => this.GetBrowserLauncher<ServiceSettingsModel>();

    [Required]
    [JsonIgnore]
    public ServiceSettingsModel WebLoader => this.GetWebLoader<ServiceSettingsModel>();

    bool? IShopSettings.IsActual { get; set; }

    public string? FileName { get; set; }

    ShopSettingType IShopSettings.Type { get => ShopSettingType; set {; } }

    [JsonIgnore]
    IDictionary IImportSettings.Services => Services;
    
    string IShopImportSettings.ShopName { get; set; }
    string IShopImportSettings.ShopUrl { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        foreach (var service in Services.Where(s => s.Key.IsPrimaryServiceName() && string.IsNullOrEmpty(s.Value.Name)))
            service.Value.Name = service.Key;
    }
}