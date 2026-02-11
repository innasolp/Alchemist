using Alchemist.Product.Data;
using Autofac;
using Mediator.Infrastructure.EF;
using Mediator.Infrastructure.Events;
using Mediator.Messages;
using Mediator.Module.EF;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShopSettings.Infrastructure;
using ShopSettings.Infrastructure.EF;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Module;

public class ShopSettingsModule : MediatorModule
{
    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetChildSettingsRequest>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(AlchemyContext)).As(typeof(DbContext));

        builder.RegisterType(typeof(EFUnitOfWork<AlchemyContext>)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterType(typeof(ShopSettingsRepository)).As(typeof(IShopSettingsRepository));

        builder.RegisterType(typeof(SaveShopSettingsCommandHandler<IDbContextTransaction>))
            .As(typeof(IRequestHandler<SaveShopSettingsCommand, Alchemist.Product.Data.ShopSettings>));
        builder.RegisterType(typeof(SaveShopSettingsWithChildrenCommandHandler<IDbContextTransaction>))
            .As(typeof(IRequestHandler<SaveShopSettingsWithChildrenCommand, 
            (Alchemist.Product.Data.ShopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings>)>));

        builder.RegisterType(typeof(MessageEventHandler<CreationEvent<Alchemist.Product.Data.ShopSettings>, Alchemist.Product.Data.ShopSettings>))
            .As(typeof(INotificationHandler<CreationEvent<Alchemist.Product.Data.ShopSettings>>));
    }
}