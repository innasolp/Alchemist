using Alchemist.Import.Settings.Interfaces;
using System.Collections;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Model;

public abstract class ShopImportSettings : IShopImportSettings
{
    public ImportServiceSettings ImportService { get; set; }

    public string? Url { get; set; }

    public ImportServiceSettings? RequestHeaders { get; set; }

    public ImportServiceSettings WebLoader { get; set; }

    public ImportServiceSettings? BrowserDataLoader { get; set; }

    public bool? Perfomance { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int ShopId { get; set; }
    
    public List<ImportServiceSettings> Services { get; set; } = [];

    IImportServiceSettings IShopImportSettings.WebLoader
    {
        get => WebLoader; set => WebLoader = (ImportServiceSettings)value;
    }

    IImportServiceSettings IShopImportSettings.ImportService
    {
        get => ImportService; set => ImportService = (ImportServiceSettings)value;
    }

    IImportServiceSettings? IShopImportSettings.BrowserDataLoader
    {
        get => BrowserDataLoader; set => BrowserDataLoader = (ImportServiceSettings?)value;
    }

    IImportServiceSettings? IShopImportSettings.RequestHeaders
    {
        get => RequestHeaders; set => RequestHeaders = (ImportServiceSettings?)value;
    }

    IList IShopImportSettings.Services => Services;

    int? ISettings.ParentSettingsId 
    { 
        get { return null; }
        set {; }
    }

    protected abstract ShopSettingType ShopSettingType { get; }

    ShopSettingType ISettings.ShopSettingType => ShopSettingType;

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
}
