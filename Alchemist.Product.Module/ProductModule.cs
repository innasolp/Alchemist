using Alchemist.Product.Infrastructure;
using Alchemist.Product.UnitOfWork;
using Alchemist.Product.UnitOfWork.Interfaces;
using Autofac;
using Mediator.Module.EF;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.Module;

public class ProductModule : MediatorModule
{
    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetCurrencyByCodeRequestHandler>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ProductUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(AlchemyRepository<>)).As(typeof(IRepository<>));
        builder.RegisterType(typeof(ShopProductRepository)).As(typeof(IShopProductRepository));
        builder.RegisterType(typeof(ProductComponentRepository)).As(typeof(IProductComponentRepository));
        builder.RegisterType(typeof(ProductPurposeRepository)).As(typeof(IProductPurposeRepository));
        builder.RegisterType(typeof(ProductPurposeTypeRepository)).As(typeof(IProductPurposeTypeRepository));
        builder.RegisterType(typeof(ProductRepository)).As(typeof(IProductRepository));
        builder.RegisterType(typeof(ShopProductPriceRepository)).As(typeof(IShopProductPriceRepository));
        builder.RegisterType(typeof(CurrencyRepository)).As(typeof(ICurrencyRepository));
        builder.RegisterType(typeof(ShopProductCategoryRepository)).As(typeof(IShopProductCategoryRepository));
    }
}