using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ProductUnitOfWork(AlchemyContext context) : EFUnitOfWork<AlchemyContext>(context), IUnitOfWork<IDbContextTransaction>// IProductUnitOfWork
{
    //public IProductRepository ProductRepository { get; } = new ProductRepository(context);

    //public IShopProductRepository ShopProductRepository { get; } = new ShopProductRepository(context);

    //public IShopProductPriceRepository ShopProductPriceRepository { get; } = new ShopProductPriceRepository(context);

    //public IShopProductCategoryRepository ShopProductCategoryRepository { get; } = new ShopProductCategoryRepository(context);

    //public IProductComponentRepository ProductComponentRepository { get; } = new ProductComponentRepository(context);

    //public ICurrencyRepository CurrencyRepository { get; } = new CurrencyRepository(context);

    //public IRepository<Brand> BrandRepository { get; } = new Repository<Brand, AlchemyContext>(context);

    //public IRepository<PurposeType> PurposeTypeRepository { get; } = new Repository<PurposeType, AlchemyContext>(context);
}
