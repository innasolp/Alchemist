using Alchemist.Import.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Shop.Interfaces;

public interface IShopImportService: IImportService
{
    IShop ShopModel { get; }
}