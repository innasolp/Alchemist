using Alchemist.Product.Entities;

namespace Import.Service.Infrastructure;

public interface IShopImportServiceRepository : IServiceRepository
{
    Task AddShopCategory(ShopCategory shopCategory);
}