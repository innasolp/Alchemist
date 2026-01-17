using Alchemist.Product.Data;
using Autofac;
using Mediator.Module.EF;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ShopSettings.Infrastructure;
using ShopSettings.UnitOfWork;
using UnitOfWork;

namespace ShopSettings.Module;

public class ShopSettingsModule : MediatorModule
{
    public override void ConfigureMediator(MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetChildSettingsRequest>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(EFUnitOfWork<AlchemyContext>)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(ShopSettingsRepository<>)).As(typeof(IRepository<>));
        builder.RegisterType(typeof(ShopSettingsRepository)).As(typeof(IShopSettingsRepository));
    }
}