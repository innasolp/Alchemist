using Alchemist.Product.Entities;

namespace Import.Service.Commands;

public interface IShopImportServiceRepository : IServiceRepository
{
    Task AddShopCategory(ShopCategory shopCategory);
}