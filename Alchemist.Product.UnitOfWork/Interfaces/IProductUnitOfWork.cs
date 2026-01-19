using Alchemist.Product.Data;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

//public interface IProductUnitOfWork : IUnitOfWork<IDbContextTransaction>
//{
//     IProductRepository ProductRepository { get; }

//     IShopProductRepository ShopProductRepository { get; }

//     IShopProductPriceRepository ShopProductPriceRepository { get; }

//     IShopProductCategoryRepository ShopProductCategoryRepository { get; }

//     IProductComponentRepository ProductComponentRepository { get; }

//     ICurrencyRepository CurrencyRepository { get; }

//     IRepository<Brand> BrandRepository { get; }

//     IRepository<PurposeType> PurposeTypeRepository { get; }
//}