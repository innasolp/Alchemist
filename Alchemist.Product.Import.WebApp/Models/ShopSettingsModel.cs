using Alchemist.Product.Import.WebApp.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ShopSettingsModel: SettingsModelBase
{
    public List<ServiceSettingsModel> ServiceSettings { get;  } = [];

    [JsonIgnore]
    public override SettingsType Type => SettingsType.Shop;

    public string? Caption { get; set; }

    public string? Url { get; set; }

    public string? RequestHeaders { get; set; }

    public ServiceSettingsModel ImportService { get; set; }

    public ServiceSettingsModel BrowserDataLoader { get; set; }

    public ServiceSettingsModel WebLoader { get; set; }

    public bool? Perfomance { get; set; }
}
