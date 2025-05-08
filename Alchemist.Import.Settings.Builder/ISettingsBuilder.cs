using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Builders;

public interface ISettingsBuilder
{    
    int Priority { get; }

    Task<IShopImportSettings?> Build(string shopSettingsName, ShopSettingType shopSettingType);
}
