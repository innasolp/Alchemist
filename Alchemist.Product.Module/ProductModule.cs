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
        builder.Register(c =>
        {
            var factory = c.Resolve<IDbContextFactory<AlchemyContext>>();
            return factory.CreateDbContext();
        })
        .AsSelf()
        .As<DbContext>()
        .InstancePerLifetimeScope();

        builder.RegisterType(typeof(ShopProductRepository)).As(typeof(IShopProductRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ProductComponentRepository)).As(typeof(IProductComponentRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ProductPurposeRepository)).As(typeof(IProductPurposeRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ProductPurposeTypeRepository)).As(typeof(IProductPurposeTypeRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ProductRepository)).As(typeof(IProductRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ShopProductPriceRepository)).As(typeof(IShopProductPriceRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(CurrencyRepository)).As(typeof(ICurrencyRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ShopProductCategoryRepository)).As(typeof(IShopProductCategoryRepository)).InstancePerLifetimeScope();
    }
}