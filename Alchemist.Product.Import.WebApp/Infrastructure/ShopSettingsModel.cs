namespace Alchemist.Product.Import.WebApp.Infrastructure;

public class ShopSettingsModel: SettingsModelBase
{
    public List<ServiceSettingsModel> ServiceSettings { get; set; }

    public override SettingsType Type => SettingsType.Shop;
}
