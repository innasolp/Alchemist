using Autofac;

namespace Db.Infrastructure.Messages;

public static class DIExtensions
{
    public static void AddBackgroundMessageHandlers(this ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterGeneric(typeof(BackgroundMessageHandler<,>)).As(typeof(IEventHandler<,>));
    }
}