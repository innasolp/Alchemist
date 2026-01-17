using Autofac;
using Microsoft.EntityFrameworkCore.Storage;
using Shop.UnitOfWork;
using UnitOfWork;
using Mediator.Module.EF;
using Shop.Infrastructure;

namespace Shop.Module;

public class ShopModule : MediatorModule
{
    public override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetAllCategoryChildrenRequest>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ShopUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(ShopRepository<>)).As(typeof(IRepository<>));
        builder.RegisterType(typeof(ShopRepository)).As(typeof(IShopRepository));
        builder.RegisterType(typeof(ShopCategoryRepository)).As(typeof(IShopCategoryRepository));
    }
}