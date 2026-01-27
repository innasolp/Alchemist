using Alchemist.Product.Data;
using Autofac;
using Mediator.Infrastructure.Command;
using Mediator.Messages;
using Mediator.Module.EF;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Shop.Infrastructure;
using Shop.UnitOfWork;
using UnitOfWork;

namespace Shop.Module;

public class ShopModule  : MediatorModule
{
    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetAllCategoryChildrenRequest>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ShopUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(ShopRepository<>)).As(typeof(IRepository<>));
        builder.RegisterType(typeof(ShopRepository)).As(typeof(IShopRepository));
        builder.RegisterType(typeof(ShopCategoryRepository)).As(typeof(IShopCategoryRepository));

        builder.RegisterType(typeof(CreateShopCommandHandler))
            .As(typeof(IRequestHandler<CreateCommand<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop>));
        builder.RegisterType(typeof(CreateShopCategoryCommandHandler))
            .As(typeof(IRequestHandler<CreateCommand<ShopCategory>, ShopCategory>));

        builder.RegisterType(typeof(BackgroundMessageEventHandler<CreateShopEvent, Alchemist.Product.Data.Shop>)).As(typeof(INotificationHandler<CreateShopEvent>));
        builder.RegisterType(typeof(BackgroundMessageEventHandler<CreateShopCategoryEvent, ShopCategory>)).As(typeof(INotificationHandler<CreateShopCategoryEvent>));
    }
}