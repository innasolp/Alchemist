using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public abstract class ShopSettingsModel : SettingsModelBase, IShopServicesSettingsModel
{
    public int Id { get; set; }

    public override string ToString()
    {
        return  @$"{base.ToString()};{nameof(ISettings.ShopId)}:{((ISettings)this).ShopId};
                  {nameof(ImportService)}:{GetServiceValueString(ImportService)};
                  {nameof(BrowserDataLoader)}:{GetServiceValueString(BrowserDataLoader)};
                  {nameof(RequestHeaders)}:{GetServiceValueString(RequestHeaders)};
                  {nameof(WebLoader)}:{GetServiceValueString(WebLoader)}";
    }

    private static string GetServiceValueString(ServiceSettingsModel? service)
    {
        return $"{service?.ServiceTypeName ?? service?.AssemblyPath}";
    }

    public List<ServiceSettingsModel> Services { get; set; } = [];

    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public abstract ShopSettingType ShopSettingType { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    [JsonInclude]
    public ServiceSettingsModel? RequestHeaders { get; private set; }

    [Required]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    [JsonInclude]
    public ServiceSettingsModel ImportService { get; private set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    [JsonInclude]
    public ServiceSettingsModel? BrowserDataLoader { get; private set; }

    [Required]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    [JsonInclude]
    public ServiceSettingsModel WebLoader { get; private set; }

    public bool? Perfomance { get; set; }

    public string? FileName { get; set; }
   
    IServiceSettingsModel IShopServicesSettingsModel.ImportService { get => ImportService;  }
    IServiceSettingsModel? IShopServicesSettingsModel.RequestHeaders { get => RequestHeaders; }
    IServiceSettingsModel IShopServicesSettingsModel.WebLoader { get => WebLoader;  }
    IServiceSettingsModel? IShopServicesSettingsModel.BrowserDataLoader { get => BrowserDataLoader;  }

    IImportServiceSettings IShopImportSettings.ImportService { get => ImportService; set { } }
    IImportServiceSettings? IShopImportSettings.RequestHeaders { get => RequestHeaders; set { } }
    IImportServiceSettings IShopImportSettings.WebLoader { get => WebLoader; set { } }
    IImportServiceSettings? IShopImportSettings.BrowserDataLoader { get => BrowserDataLoader; set { } }

    IList IShopImportSettings.Services => Services;

    string IShopImportSettings.ShopName { get => null; set { } }

    string IShopImportSettings.ShopUrl { get => null; set { } }

    int ISettings.ShopId { get => ShopId; set { } }

    int? ISettings.ParentSettingsId { get => null; set  { } }

    public ShopSettingsModel(int shopId, int id, Guid shopGuid) : base(shopId, shopGuid)
    {
        Id = id;
        ImportService = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid, nameof(IShopServicesSettingsModel.ImportService));
        BrowserDataLoader = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid, nameof(IShopServicesSettingsModel.BrowserDataLoader));
        RequestHeaders = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid, nameof(IShopServicesSettingsModel.RequestHeaders));
        WebLoader = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid, nameof(IShopServicesSettingsModel.WebLoader));
    }    
}
