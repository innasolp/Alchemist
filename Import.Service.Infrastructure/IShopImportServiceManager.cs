using Alchemist.Product.Entities;

namespace Import.Service.Infrastructure;

public interface IShopImportServiceManager : IServiceManager
{
    Task AddShopCategory(ShopCategory shopCategory);
}