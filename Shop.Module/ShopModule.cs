using Alchemist.Product.Data;
using Autofac;
using Mediator.Infrastructure.Command;
using Mediator.Messages;
using Mediator.Module.EF;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.Infrastructure;
using Shop.UnitOfWork;
using UnitOfWork;

namespace Shop.Module;

public class ShopModule (string logCategory) : MediatorModule
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationPublisher, LoggingNotificationPublisher<ForeachAwaitPublisher>>(serviceProvider =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(logCategory);
            return new LoggingNotificationPublisher<ForeachAwaitPublisher>(logger);
        });

        base.ConfigureServices(services);
    }

    protected override void ConfigureMediator(Microsoft.Extensions.DependencyInjection.MediatRServiceConfiguration cfg)
    {
        cfg.RegisterServicesFromAssemblyContaining<GetAllCategoryChildrenRequest>();
    }

    protected override void RegisterTypes(ContainerBuilder builder)
    {
        builder.RegisterType(typeof(ShopUnitOfWork)).As(typeof(IUnitOfWork<IDbContextTransaction>));
        builder.RegisterGeneric(typeof(ShopRepository<>)).As(typeof(IRepository<>));
        builder.RegisterType(typeof(ShopRepository)).As(typeof(IShopRepository));
        builder.RegisterType(typeof(ShopCategoryRepository)).As(typeof(IShopCategoryRepository));

        builder.RegisterType(typeof(CreateShopCommandHandler))
            .As(typeof(IRequestHandler<CreateCommand<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop>));
        builder.RegisterType(typeof(CreateShopCategoryCommandHandler))
            .As(typeof(IRequestHandler<CreateCommand<ShopCategory>, ShopCategory>));

        builder.RegisterType(typeof(MessageEventHandler<CreateShopEvent, Alchemist.Product.Data.Shop>)).As(typeof(INotificationHandler<CreateShopEvent>));
        builder.RegisterType(typeof(MessageEventHandler<CreateShopCategoryEvent, ShopCategory>)).As(typeof(INotificationHandler<CreateShopCategoryEvent>));
    }
}