using Alchemist.Product.Import.Model;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Model.ShopSettings;

public interface IShopSettingsModel : ISettingsModel, IShopSettings
{
    string? FileName { get; set; }
}
