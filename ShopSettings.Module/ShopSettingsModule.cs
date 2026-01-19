using Alchemist.Product.Data;
using Autofac;
using Mediator.Messages;
using Mediator.Module.EF;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using ShopSettings.Infrastructure;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Module;

public class ShopSettingsModule : MediatorModule
{
    public override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetChildSettingsRequest>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(EFUnitOfWork<AlchemyContext>)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(ShopSettingsRepository<>)).As(typeof(IRepository<>));
        builder.RegisterType(typeof(ShopSettingsRepository)).As(typeof(IShopSettingsRepository));

        builder.RegisterType(typeof(SaveShopSettingsCommandHandler))
            .As(typeof(IRequestHandler<SaveShopSettingsCommand, Alchemist.Product.Data.ShopSettings>));
        builder.RegisterType(typeof(SaveShopSettingsWithChildrenCommandHandler))
            .As(typeof(IRequestHandler<SaveShopSettingsWithChildrenCommand, 
            (Alchemist.Product.Data.ShopSettings, IEnumerable<Alchemist.Product.Data.ShopSettings>)>));

        builder.RegisterType(typeof(MessageEventHandler<CreateShopSettingsEvent, Alchemist.Product.Data.ShopSettings>))
            .As(typeof(INotificationHandler<CreateShopSettingsEvent>));
    }
}