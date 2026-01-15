using Autofac;
using Mediator.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Shop.UnitOfWork;
using UnitOfWork;

namespace Shop.Module;

public class ShopModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ShopUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(ShopRepository<>)).As(typeof(IRepository<>));
        builder.RegisterGeneric(typeof(EFCreateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(EFUpdateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(GetByIdRequestHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(FindByNameRequestHandler<,>)).As(typeof(IRequestHandler<,>));
    }
}