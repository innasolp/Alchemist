using Alchemist.Import.Settings.Interfaces;
using System.Collections;

namespace Alchemist.Product.Import.Model;

public interface ICategoryUrlModel : ICategoryUrl, IModel
{  
    Guid ShopSettingsGuid { get; }
}

public interface IProductShopSettingsModel : IShopImportSettingsModel, IProductShopImportSettings
{
    new IList RootCategories { get; }
}
