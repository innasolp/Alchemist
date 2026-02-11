using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure;
using Alchemist.Product.Infrastructure.EF;
using Alchemist.Product.Infrastructure.Interfaces;
using Autofac;
using Mediator.Module.EF;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Module;

public class ProductModule : MediatorModule
{
    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetCurrencyByCodeRequestHandler>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(AlchemyContext)).As(typeof(DbContext));

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