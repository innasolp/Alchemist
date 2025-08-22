using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Product.Import.WebApp.Models;

public abstract partial class ShopSettingsModel : SettingsModelBase, IShopImportSettingsModel
{
    public int Id { get; set; }

    public override string ToString()
    {
        return  @$"{base.ToString()};{nameof(ISettings.ShopId)}:{((ISettings)this).ShopId};
                  {nameof(ImportService)}:{GetServiceValueString(ImportService)};
                  {nameof(BrowserDataLoader)}:{GetServiceValueString(BrowserDataLoader)};
                  {nameof(BrowserLauncher)}:{GetServiceValueString(BrowserLauncher)};
                  {nameof(RequestHeaders)}:{GetServiceValueString(RequestHeaders)};
                  {nameof(WebLoader)}:{GetServiceValueString(WebLoader)}";
    }

    private static string GetServiceValueString(ServiceSettingsModel? service)
    {
        return $"{service?.ServiceTypeName ?? service?.AssemblyPath}";
    }

    [JsonInclude]
    public SuppressibleObservableCollection<ServiceSettingsModel> Services { get; private set; } = [];

    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public abstract ShopSettingType ShopSettingType { get; }

    [JsonIgnore]
    public ServiceSettingsModel? RequestHeaders => this.GetRequestHeaders() as ServiceSettingsModel;

    [Required]
    [JsonIgnore]
    public ServiceSettingsModel ImportService => this.GetImportService() as ServiceSettingsModel;

    [JsonIgnore]
    public ServiceSettingsModel? BrowserDataLoader => this.GetBrowserDataLoader() as ServiceSettingsModel;

    [JsonIgnore]
    public ServiceSettingsModel? BrowserLauncher => this.GetBrowserLauncher() as ServiceSettingsModel;

    [Required]
    [JsonIgnore]
    public ServiceSettingsModel WebLoader => this.GetWebLoader() as ServiceSettingsModel;

    public bool? Perfomance { get; set; }

    public string? FileName { get; set; }  


    IList IShopImportSettings.Services => Services;

    string IShopImportSettings.ShopName { get => null; set { } }

    string IShopImportSettings.ShopUrl { get => null; set { } }   

    int? ISettings.ParentSettingsId { get => null; set  { } }

    public ShopSettingsModel(int shopId, int id, Guid shopGuid) : base(shopId, shopGuid)
    {
        Id = id;

        Services.SuspendNotifications();
        foreach(var serviceName in SettingsCommon.GetPrimaryServiceNames())
        {
            var service = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid) { Name = serviceName };
            Services.Add(service);
        }
        Services.ResumeNotifications();
    }    
}
