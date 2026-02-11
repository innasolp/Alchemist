using Autofac;
using Mediator.Infrastructure.EF;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Module.EF;

public abstract class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        RegisterTypes(builder);

        builder.RegisterGeneric(typeof(EFUnitOfWork<>)).As(typeof(EFUnitOfWork<>));

        builder.RegisterGeneric(typeof(CreateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(UpdateCommandHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(GetByIdRequestHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(FindByNameRequestHandler<,>)).As(typeof(IRequestHandler<,>));
        builder.RegisterGeneric(typeof(GetAllRequestHandler<,>)).As(typeof(IRequestHandler<,>));
    }

    protected abstract void RegisterTypes(ContainerBuilder builder);

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            ConfigureMediator(cfg);
        });
    }

    protected abstract void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration configuration);
}