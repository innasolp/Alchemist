using Alchemist.Import.Settings.Interfaces;
using System.Collections;

namespace Alchemist.Product.Import.Model;

public interface ICategoryUrlModel : ICategoryUrl
{
    Guid Guid { get; }   

    Guid ShopSettingsGuid { get; }
}

public interface IProductShopSettingsModel : IShopServicesSettingsModel, IProductShopImportSettings
{
    new IList RootCategories { get; }
}
