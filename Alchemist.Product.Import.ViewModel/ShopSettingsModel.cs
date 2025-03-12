using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public abstract class ShopSettingsModel : SettingsModelBase, IShopImportSettings{
    
    public List<ServiceSettingsModel> Services { get; set; } = [];

    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public abstract ShopSettingType ShopSettingType { get; }

    [Required]
    public string? Caption { get; set; }

    [Required]
    public string? Url { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    public ServiceSettingsModel? RequestHeaders { get; set; }

    [Required]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    public ServiceSettingsModel ImportService { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    public ServiceSettingsModel? BrowserDataLoader { get; set; }

    [Required]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    public ServiceSettingsModel WebLoader { get; set; }

    public bool? Perfomance { get; set; }

    public string? FileName { get; set; }
   
    IImportServiceSettings IShopImportSettings.ImportService { get => ImportService; set => ImportService = (ServiceSettingsModel)value; }
    IImportServiceSettings? IShopImportSettings.RequestHeaders { get => RequestHeaders; set => RequestHeaders = (ServiceSettingsModel)value; }
    IImportServiceSettings IShopImportSettings.WebLoader { get => WebLoader; set => WebLoader = (ServiceSettingsModel)value; }
    IImportServiceSettings? IShopImportSettings.BrowserDataLoader { get => BrowserDataLoader; set => BrowserDataLoader = (ServiceSettingsModel)value; }

    IList IShopImportSettings.Services => Services;
    
    int? ISettings.ParentSettingsId { get =>null; set {; } }

    public override void Update(SettingsModelBase source)
    {
        base.Update(source);

        if (source is not ShopSettingsModel sourceShopSettings) return;
        Update(sourceShopSettings, false);
    }

    public virtual void Update(ShopSettingsModel sourceShopSettings, bool setNullServices = false)
    {        
        Name = sourceShopSettings.Name;
        Caption = sourceShopSettings.Caption;
        Url = sourceShopSettings.Url;
        Perfomance = sourceShopSettings.Perfomance;
        FileName = sourceShopSettings.FileName;

        if (sourceShopSettings.ImportService != null)
        {
            SetServiceSettings(sourceShopSettings.ImportService, nameof(ImportService), () => ImportService, value => ImportService = value);
        }
        else if (setNullServices)
        {
            Services.Remove(ImportService);
            ImportService = null;
        }

        if (sourceShopSettings.BrowserDataLoader != null)
        {
            SetServiceSettings(sourceShopSettings.BrowserDataLoader, nameof(BrowserDataLoader), () => BrowserDataLoader, value => BrowserDataLoader = value);
        }
        else if (setNullServices)
        {
            Services.Remove(BrowserDataLoader);
            BrowserDataLoader = null;
        }

        if (sourceShopSettings.WebLoader != null)
        {
            SetServiceSettings(sourceShopSettings.WebLoader, nameof(WebLoader), () => WebLoader, value => WebLoader = value);
        }
        else if (setNullServices)
        {
            Services.Remove(WebLoader);
            WebLoader = null;
        }

        if (sourceShopSettings.RequestHeaders != null)
        {
            SetServiceSettings(sourceShopSettings.RequestHeaders, nameof(RequestHeaders), () => RequestHeaders, value => RequestHeaders = value);
        }
        else if (setNullServices)
        {
            Services.Remove(RequestHeaders);
            RequestHeaders = null;
        }

        
        sourceShopSettings.Services.Where(s=> !Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name)).ToList().ForEach(SetServiceSettings);
        if (setNullServices)
        {
            Services.RemoveAll(s => !Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name) && 
            !sourceShopSettings.Services.Any(source => IsEqualService(source, s)));
        }
    }

    private void SetServiceSettings(ServiceSettingsModel source, string name, Func<ServiceSettingsModel> get, Action<ServiceSettingsModel> set)
    {
        if (get() == null)
            set(new ServiceSettingsModel
            {
                ShopGuid = ShopGuid,
                Name = name,
                ShopSettingsGuid = Guid,
                ParentSettingsId = Id,
                ShopId = ShopId
            });

        get().Update(source);

        if (!Services.Any(s => s.Guid == get().Guid))
            Services.Add(get());
    }

    private bool IsEqualService(ServiceSettingsModel source, ServiceSettingsModel target)
    {
        return target.Guid == source.Guid
        || (!string.IsNullOrEmpty(target.Name) && target.Name == source.Name)
        || target.ServiceTypeName == source.ServiceTypeName;
    }

    private void SetServiceSettings(ServiceSettingsModel service)
    {
        var serviceSettings = Services.FirstOrDefault(s => IsEqualService(service,s))
            ?? new ServiceSettingsModel
            {
                ShopGuid = ShopGuid,
                ShopSettingsGuid = Guid,
                ParentSettingsId = Id,
                ShopId = ShopId,
                Name = service.Name
            };

        serviceSettings.Update(service);

        if (!Services.Any(s => s.Guid == serviceSettings.Guid))
            Services.Add(serviceSettings);
    }
}
