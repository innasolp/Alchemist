using Message.Interfaces;
using Message.RabbitMQ;
using Message.RabbitMQ.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.RabbitMQ;

public static class RabbitMqExtensions
{
    public static IServiceCollection SetRabbitMqSender(this IServiceCollection services, Uri uri, string exchangeName)
    {
        var rabbitMqSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(RabbitMQPublisher)).ToList();
        rabbitMqSenderDescriptors.ForEach(d => services.Remove(d));

        var messageSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender) && sd.ImplementationType == typeof(RabbitMQPublisher)).ToList();
        messageSenderDescriptors.ForEach(d => services.Remove(d));

        var messageSenderFactoryDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender) 
        && sd.ImplementationFactory?.Method.ReturnType == typeof(RabbitMQPublisher)).ToList();
        messageSenderFactoryDescriptors.ForEach(d => services.Remove(d));

        services.AddRabbitMQMessageSender(uri, exchangeName);

        return services;
    }
    public static IServiceCollection SetRabbitMqSender(this IServiceCollection services, object key, Uri uri, string exchangeName)
    {
        var rabbitMqSenderDescriptors = services.Where(sd => sd.ServiceKey == key &&  sd.ServiceType == typeof(RabbitMQPublisher)).ToList();
        rabbitMqSenderDescriptors.ForEach(d => services.Remove(d));

        var messageSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender) && sd.ServiceKey == key).ToList();
        messageSenderDescriptors.ForEach(d => services.Remove(d));        

        services.AddRabbitMQMessageSender(key, uri, exchangeName);

        return services;
    }

}
