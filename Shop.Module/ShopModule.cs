using Alchemist.Product.Data;
using Autofac;
using Mediator.Infrastructure;
using Mediator.Infrastructure.EF;
using Mediator.Infrastructure.Events;
using Mediator.Messages;
using Mediator.Module.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure;
using Shop.Infrastructure.EF;

namespace Shop.Module;

public class ShopModule  : MediatorModule
{
    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetAllCategoryChildrenRequest>();
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

        builder.RegisterType(typeof(ShopRepository)).As(typeof(IShopRepository)).InstancePerLifetimeScope();
        builder.RegisterType(typeof(ShopCategoryRepository)).As(typeof(IShopCategoryRepository)).InstancePerLifetimeScope();     
        
        builder.RegisterGeneric(typeof(CreateCommandHandler<>)).As(typeof(ICreateCommandHandler<>));
        
        builder.RegisterType(typeof(CreateEventedCommandHandler<Alchemist.Product.Data.Shop>))
            .WithParameter("eventName", Messages.ShopCreated)
            .As(typeof(IRequestHandler<CreateCommand<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop>));
        
        builder.RegisterType(typeof(CreateEventedCommandHandler<ShopCategory>))
            .WithParameter("eventName", Messages.CategoryAdded)
            .As(typeof(IRequestHandler<CreateCommand<ShopCategory>, ShopCategory>));

        builder.RegisterType(typeof(BackgroundMessageEventHandler<CreationEvent<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop>))
            .As(typeof(INotificationHandler<CreationEvent<Alchemist.Product.Data.Shop>>));

        builder.RegisterType(typeof(BackgroundMessageEventHandler<CreationEvent<ShopCategory>, ShopCategory>))
            .As(typeof(INotificationHandler<CreationEvent<ShopCategory>>));
    }
}