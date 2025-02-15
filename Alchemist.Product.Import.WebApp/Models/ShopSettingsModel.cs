using Alchemist.Product.Import.WebApp.Infrastructure;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ShopSettingsModel: SettingsModelBase
{
    public List<ServiceSettingsModel> ServiceSettings { get; set; }

    [JsonIgnore]
    public override SettingsType Type => SettingsType.Shop;
}
