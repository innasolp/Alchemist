using Alchemist.Product.Data;
using Autofac;
using Db.Infrastructure;
using Db.Infrastructure.EF;
using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class ProductDataModule : Module
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
        .As<AlchemyContext>()
        .InstancePerLifetimeScope();

        builder.RegisterType(typeof(EFUnitOfWork<AlchemyContext>)).As(typeof(IUnitOfWork));

        builder.RegisterGeneric(typeof(CreateCommandHandler<>)).As(typeof(ICommandHandler<>))
            .AsImplementedInterfaces().InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(UpdateCommandHandler<>)).As(typeof(ICommandHandler<>))
            .AsImplementedInterfaces().InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(FindByNameRequestHandler<>)).As(typeof(IRequestHandler<,>)).InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(ProductDataModule).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(typeof(EFUnitOfWork<>).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}