using Message.Interfaces;
using Message.SignalR;
using Message.SignalR.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.SignalRWebAppFactory;

public static class SignalRDependencyInjectionExtensions
{
    public static IServiceCollection SetSignalRTestSender(this IServiceCollection services, TestServer signalRServer, string[] hubs)
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection)).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        var messageSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender)
                && sd.ImplementationType == typeof(SignalRMessageSender)).ToList();
        messageSenderDescriptors.ForEach(d => services.Remove(d));

        var messageSenderFactoryDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender)
                && sd.ImplementationFactory?.Method.ReturnType == typeof(SignalRMessageSender)).ToList();
        messageSenderFactoryDescriptors.ForEach(d => services.Remove(d));

        var handler = signalRServer.CreateHandler();

        foreach (var hub in hubs)
        {
            var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
            services.AddSignalRMessageSender(signalRUrl, handler);
        }

        return services;
    }

    public static IServiceCollection SetSignalRTestSender(this IServiceCollection services, object key, TestServer signalRServer, string hub)
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection) &&
                    sd.IsKeyedService && sd.ServiceKey == key).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        var messageSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender)
                && sd.IsKeyedService && sd.ServiceKey == key
                && sd.ImplementationType == typeof(SignalRMessageSender)).ToList();
        messageSenderDescriptors.ForEach(d => services.Remove(d));

        var messageSenderFactoryDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender)
                && sd.IsKeyedService && sd.ServiceKey == key
                && sd.ImplementationFactory?.Method.ReturnType == typeof(SignalRMessageSender)).ToList();
        messageSenderFactoryDescriptors.ForEach(d => services.Remove(d));

        var handler = signalRServer.CreateHandler();
        var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
        services.AddKeyedSignalRMessageSender(signalRUrl, handler, key);

        return services;
    }

    public static IServiceCollection SetSignalRTestReceiver(this IServiceCollection services, object key, TestServer signalRServer, string hub)
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection) &&
                    sd.IsKeyedService && sd.ServiceKey == key).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        var messageSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageReceiver)
                && sd.IsKeyedService && sd.ServiceKey == key
                && sd.ImplementationType == typeof(SignalRMessageReceiver)).ToList();
        messageSenderDescriptors.ForEach(d => services.Remove(d));

        var messageSenderFactoryDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageReceiver)
                && sd.IsKeyedService && sd.ServiceKey == key
                && sd.ImplementationFactory?.Method.ReturnType == typeof(SignalRMessageReceiver)).ToList();
        messageSenderFactoryDescriptors.ForEach(d => services.Remove(d));

        var handler = signalRServer.CreateHandler();
        var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
        services.AddKeyedSignalRMessageReceiver(signalRUrl, handler, key);

        return services;
    }
}
