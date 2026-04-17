using Alchemist.Product.Data;
using Autofac;
using Db.Infrastructure;
using Db.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Shop.Data.Infrastructure.EF;

public class ShopDataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c =>
        {
            var factory = c.Resolve<IDbContextFactory<AlchemyContext>>();
            return factory.CreateDbContext();
        })
        .AsSelf()
        .As<DbContext>()
        .InstancePerLifetimeScope();

        builder.RegisterType(typeof(EFUnitOfWork<AlchemyContext>)).As(typeof(IUnitOfWork));

        builder.RegisterGeneric(typeof(CreateCommandHandler<>)).As(typeof(ICommandHandler<>))
            .AsImplementedInterfaces().InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(UpdateCommandHandler<>)).As(typeof(ICommandHandler<>))
            .AsImplementedInterfaces().InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(ShopDataModule).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(EFUnitOfWork<>).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}