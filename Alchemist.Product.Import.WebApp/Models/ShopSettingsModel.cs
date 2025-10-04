using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
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
        return  @$"{base.ToString()};{nameof(IShopSettings.ShopId)}:{((IShopSettings)this).ShopId};
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
    public Dictionary<string, ServiceSettingsModel> Services { get; private set; } = [];

    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public abstract ShopSettingType Type { get; }

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

    public bool? Perfomance { get; set; }

    public string? FileName { get; set; }  


    IDictionary IShopImportSettings.Services => Services;

    string IShopImportSettings.ShopName { get => null; set { } }

    string IShopImportSettings.ShopUrl { get => null; set { } }   

    int? IShopSettings.ParentSettingsId { get => null; set  { } }

    [JsonIgnore]
    bool? IShopSettings.IsActual { get ; set; }

    [JsonIgnore]
    string IShopSettings.JsonValue { get; set; }

    [JsonIgnore]
    ShopSettingType IShopSettings.Type { get => Type; set {; } }

    public ShopSettingsModel(int shopId, int id, Guid shopGuid) : base(shopId, shopGuid)
    {
        Id = id;

        //Services.SuspendNotifications();
        //foreach(var serviceName in SettingsCommon.GetPrimaryServiceNames())
        //{
        //    var service = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid) { Name = serviceName };
        //    Services.Add(service);
        //}
        //Services.ResumeNotifications();

        foreach (var serviceName in SettingsCommon.GetPrimaryServiceNames())
        {
            var service = new ServiceSettingsModel(shopId, 0, id, Guid, shopGuid) { Name = serviceName };
            Services.Add(serviceName, service);
        }
    }    
}
