using Mediator.Infrastructure.Command;

namespace ShopSettings.Infrastructure;

public class SaveShopSettingsCommand(Alchemist.Product.Data.ShopSettings shopSettings) : Command<Alchemist.Product.Data.ShopSettings>(shopSettings)
{
}

public class SaveShopSettingsWithChildrenCommand(Alchemist.Product.Data.ShopSettings parentShopSettings,
    IEnumerable<Alchemist.Product.Data.ShopSettings> childrenSettings)
    : Command<(Alchemist.Product.Data.ShopSettings shopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings> services)>((parentShopSettings, childrenSettings))
{
    public Alchemist.Product.Data.ShopSettings ParentShopSettings { get; } = parentShopSettings;

    public IEnumerable<Alchemist.Product.Data.ShopSettings> ChildrenSettings { get; } = childrenSettings;
}