using Alchemist.Product.Import.WebApp.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ShopSettingsModel: SettingsModelBase
{
    public List<ServiceSettingsModel> ServiceSettings { get; set; }

    [JsonIgnore]
    public override SettingsType Type => SettingsType.Shop;
    
    string? Caption { get; set; }

    string? Url { get; set; }

    string? RequestHeaders { get; set; }

    public ServiceSettingsModel ImportService { get; set; }

    public ServiceSettingsModel BrowserDataLoader { get; set; }

    public ServiceSettingsModel WebLoader { get; set; }

    public bool? Perfomance { get; set; }
}
