using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebLoader.Common;

namespace Alchemist.Import.Settings.Model;

public class ShopImportSettings : IShopImportSettings
{
    public required ImportServiceSettings ImportService { get; set; }

    public string? Url { get; set; }

    public ImportServiceSettings? RequestHeadersSettings { get; set; }

    public required ImportServiceSettings WebLoader { get; set; }

    public ImportServiceSettings? BrowserDataLoader { get; set; }

    public bool? Perfomance { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Caption { get; set; }

    [JsonIgnore]
    public int ShopId { get; set; }

    public RequestHeaders? RequestHeaders { get; set; }

    public List<ImportServiceSettings> Services { get; set; } = [];

    IImportServiceSettings IShopImportSettings.WebLoader => WebLoader;

    IImportServiceSettings IShopImportSettings.ImportService => ImportService;

    IImportServiceSettings? IShopImportSettings.BrowserDataLoader => BrowserDataLoader;

    IImportServiceSettings? IShopImportSettings.RequestHeadersSettings => RequestHeadersSettings;

    IEnumerable<IImportServiceSettings> IShopImportSettings.Services => Services;    

    protected static void InitializeServiceSettings(ImportServiceSettings serviceSettings, Func<int, IShopSettings> getSettingsById)
    {
        var serviceId = (int)serviceSettings.Id;
        var shopSettings = getSettingsById(serviceId);
        if(shopSettings == null)
            throw new InvalidDataException($"No shop settings for id {serviceId}");

        var serviceSettingsModel = ImportServiceSettings.Create(shopSettings);
        serviceSettings.ServiceProviderPath = serviceSettingsModel.ServiceProviderPath;
        serviceSettings.AssemblyPath = serviceSettingsModel.AssemblyPath;
        serviceSettings.ServiceTypeName = serviceSettingsModel.ServiceTypeName;
        serviceSettings.Name = serviceSettingsModel.Name;
        serviceSettings.ImplementationTypeName = serviceSettingsModel.ImplementationTypeName;
        serviceSettings.Value = serviceSettingsModel.Value;
    }   

    protected static T Create<T>(IShopSettings shopSettings, Func<int, IShopSettings> getSettingsById)
        where T : ShopImportSettings
    {
        var shopImportSettings = JsonSerializer.Deserialize<T>(shopSettings.JsonValue);
        
        shopImportSettings.Id = shopSettings.Id;
        shopImportSettings.ShopId = shopSettings.ShopId;

        if (shopImportSettings.ImportService?.Id != null)
            InitializeServiceSettings(shopImportSettings.ImportService, getSettingsById);
        
        if(shopImportSettings.RequestHeaders == null && shopImportSettings.RequestHeadersSettings != null)
            InitializeServiceSettings(shopImportSettings.RequestHeadersSettings, getSettingsById);        
        
        if(shopImportSettings.BrowserDataLoader != null)
            InitializeServiceSettings(shopImportSettings.BrowserDataLoader, getSettingsById);

        if (shopImportSettings.WebLoader != null)
            InitializeServiceSettings(shopImportSettings.WebLoader, getSettingsById);

        foreach(var service in shopImportSettings.Services)
            InitializeServiceSettings(service, getSettingsById);   

        return shopImportSettings;
    }

    public static ShopImportSettings CreateShopImportSettings(IShopSettings shopSettings, Func<int, IShopSettings> getSettingsById)
    {
        return Create<ShopImportSettings>(shopSettings, getSettingsById);
    }
}
