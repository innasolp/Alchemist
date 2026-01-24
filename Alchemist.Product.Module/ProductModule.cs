using Alchemist.Product.Infrastructure;
using Alchemist.Product.UnitOfWork;
using Autofac;
using Mediator.Module.EF;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.Module;

public class ProductModule : MediatorModule
{
    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetCurrencyByCodeRequestHandler>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ProductUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(AlchemyRepository<>)).As(typeof(IRepository<>));
    }
}