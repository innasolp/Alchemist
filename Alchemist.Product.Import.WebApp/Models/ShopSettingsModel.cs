using Alchemist.Product.Import.WebApp.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ShopSettingsModel: SettingsModelBase
{
    public List<ServiceSettingsModel> ServiceSettings { get;  } = [];

    [JsonIgnore]
    public override SettingsType Type => SettingsType.Shop;

    [Required]
    public string? Caption { get; set; }

    [Required]
    public string? Url { get; set; }

    public string? RequestHeaders { get; set; }

    [Required]
    public ServiceSettingsModel ImportService { get; set; }

    public ServiceSettingsModel? BrowserDataLoader { get; set; }

    [Required]
    public ServiceSettingsModel WebLoader { get; set; }

    public bool? Perfomance { get; set; }

    public override void Update(SettingsModelBase source)
    {
        base.Update(source);

        if (!(source is ShopSettingsModel sourceShopSettings)) return;

        Caption = sourceShopSettings.Caption;
        Url = sourceShopSettings.Url;
        RequestHeaders = sourceShopSettings.RequestHeaders;
        Perfomance = sourceShopSettings.Perfomance;

        sourceShopSettings.UpdateServiceSettings(sourceShopSettings.ImportService);
        sourceShopSettings.UpdateServiceSettings(sourceShopSettings.WebLoader);
        sourceShopSettings.UpdateServiceSettings(sourceShopSettings.BrowserDataLoader);
    }
}
