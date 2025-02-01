using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Shop.Interfaces;

public interface IShopImportService: IImportService
{
    IShopModel ShopModel { get; }
}