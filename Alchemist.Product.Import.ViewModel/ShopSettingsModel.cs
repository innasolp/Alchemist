using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model;

public abstract class ShopSettingsModel : SettingsModelBase, IShopImportSettings
{
    public List<ServiceSettingsModel> Services { get; } = [];

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

    //todo
    IList<IImportServiceSettings> IShopImportSettings.Services => [.. Services.OfType<IImportServiceSettings>()];

    public override void Update(SettingsModelBase source)
    {
        base.Update(source);

        if (source is not ShopSettingsModel sourceShopSettings) return;

        Caption = sourceShopSettings.Caption;
        Url = sourceShopSettings.Url;
        RequestHeaders = sourceShopSettings.RequestHeaders;
        Perfomance = sourceShopSettings.Perfomance;
        FileName = sourceShopSettings.FileName;

        if (sourceShopSettings.ImportService != null)
        {
            SetServiceSettings(sourceShopSettings.ImportService, nameof(ImportService), () => ImportService, value => ImportService = value);
        }

        if (sourceShopSettings.BrowserDataLoader != null)
        {
            SetServiceSettings(sourceShopSettings.BrowserDataLoader, nameof(BrowserDataLoader), () => BrowserDataLoader, value => BrowserDataLoader = value);
        }

        if (sourceShopSettings.WebLoader != null)
        {
            SetServiceSettings(sourceShopSettings.WebLoader, nameof(WebLoader), () => WebLoader, value => WebLoader = value);
        }
        
        if (sourceShopSettings.RequestHeaders != null)
        {
            SetServiceSettings(sourceShopSettings.RequestHeaders, nameof(RequestHeaders), () => RequestHeaders, value => RequestHeaders = value);
        }
    }

    private void SetServiceSettings(ServiceSettingsModel service, string name, Func<ServiceSettingsModel> get, Action<ServiceSettingsModel> set)
    {
        if (get() == null)
            set(new ServiceSettingsModel
            {
                ShopGuid = ShopGuid,
                Name = name,
                ShopSettingsGuid = Guid,
                ParentSettingsId = Id
            });

        get().Update(service);

        if (!Services.Any(s => s.Guid == get().Guid))
            Services.Add(get());
    }
}
