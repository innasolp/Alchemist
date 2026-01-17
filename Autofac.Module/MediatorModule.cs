using Autofac;
using Mediator.Infrastructure;
using MediatR;

namespace Mediator.Module.EF;

public abstract class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterTypes(builder);

        builder.RegisterGeneric(typeof(EFCreateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(EFUpdateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(GetByIdRequestHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(FindByNameRequestHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(GetAllRequestHandler<,>)).As(typeof(IRequestHandler<,>));
    }

    protected abstract void RegisterTypes(ContainerBuilder builder);

    public abstract void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration configuration);
}