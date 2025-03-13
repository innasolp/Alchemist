using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public abstract class ShopSettingsModel : SettingsModelBase, IShopImportSettings
{
    public override string ToString()
    {
        return  @$"{base.ToString()};{nameof(Caption)}:{Caption};{nameof(Url)}:{Url};{nameof(ISettings.ShopId)}:{((ISettings)this).ShopId};
                  {nameof(ImportService)}:{GetServiceValueString(ImportService)};
                  {nameof(BrowserDataLoader)}:{GetServiceValueString(BrowserDataLoader)};
                  {nameof(RequestHeaders)}:{GetServiceValueString(RequestHeaders)};
                  {nameof(WebLoader)}:{GetServiceValueString(WebLoader)}";
    }

    private string GetServiceValueString(ServiceSettingsModel? service)
    {
        return $"{service?.ServiceTypeName ?? service?.AssemblyPath}";
    }

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

    int ISettings.ShopId { get; set; }

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
            if (ImportService != null)
            {
                ImportService.Update(sourceShopSettings.ImportService);
                this.AddOrUpdateServices(ImportService);
            }
            else
                this.SetServiceSettings(sourceShopSettings.ImportService);            
        }
        else if (setNullServices)
        {
            Services.Remove(ImportService);
            ImportService = null;
        }

        if (sourceShopSettings.BrowserDataLoader != null)
        {
            if (BrowserDataLoader != null)
            {
                BrowserDataLoader.Update(sourceShopSettings.BrowserDataLoader);
                this.AddOrUpdateServices(BrowserDataLoader);
            }
            else
                this.SetServiceSettings(sourceShopSettings.BrowserDataLoader);
        }
        else if (setNullServices)
        {
            Services.Remove(BrowserDataLoader);
            BrowserDataLoader = null;
        }

        if (sourceShopSettings.WebLoader != null)
        {
            if (WebLoader != null)
            {
                WebLoader.Update(sourceShopSettings.WebLoader);
                this.AddOrUpdateServices(WebLoader);
            }
            else
                this.SetServiceSettings(sourceShopSettings.WebLoader);
        }
        else if (setNullServices)
        {
            Services.Remove(WebLoader);
            WebLoader = null;
        }

        if (sourceShopSettings.RequestHeaders != null)
        {
            if (RequestHeaders != null)
            {
                RequestHeaders.Update(sourceShopSettings.RequestHeaders);
                this.AddOrUpdateServices(RequestHeaders);
            }
            else
                this.SetServiceSettings(sourceShopSettings.RequestHeaders);
        }
        else if (setNullServices)
        {
            Services.Remove(RequestHeaders);
            RequestHeaders = null;
        }
        
        sourceShopSettings.Services.Where(s=> !Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name)).ToList()
            .ForEach(this.AddOrUpdateServices);
        if (setNullServices)
        {
            Services.RemoveAll(s => !Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name) && 
            !sourceShopSettings.Services.Any(source => source.IsEqual(s)));
        }
    }
}
