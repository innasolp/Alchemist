using Autofac;

namespace Db.Infrastructure.Messages;

public static class DIExtensions
{
    public static void AddBackgroundMessageHandlers(this ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterGeneric(typeof(BackgroundMessageHandler<,>)).As(typeof(IEventHandler<,>));
    }

    public static void AddBackgroundMessageHandler<TEvent,T>(this ContainerBuilder containerBuilder)
        where TEvent:class, IEvent<T>
    {
        containerBuilder.RegisterType(typeof(BackgroundMessageHandler<T, TEvent>)).As(typeof(IEventHandler<T, TEvent>));
    }

    public static void AddCallbackBackgroundMessageHandlers(this ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterGeneric(typeof(CallbackBackgroundMessageHandler<,>)).As(typeof(IEventHandler<,>)).SingleInstance();
    }
}