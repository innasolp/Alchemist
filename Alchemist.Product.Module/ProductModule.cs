using Alchemist.Product.UnitOfWork;
using Autofac;
using Mediator.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using UnitOfWork;

namespace Alchemist.Product.Module;

public class ProductModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ProductUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(AlchemyRepository<>)).As(typeof(IRepository<>));
        builder.RegisterGeneric(typeof(EFCreateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(EFUpdateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(GetByIdRequestHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(FindByNameRequestHandler<,>)).As(typeof(IRequestHandler<,>));
    }
}