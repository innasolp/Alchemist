using Message.Interfaces;
using Message.SignalR.DependencyInjection;
using Message.SignalR.HubMessage.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.Server.Fixtures;

namespace Alchemist.Test.SignalRWebAppFactory;

public static class SignalRDependencyInjectionExtensions
{
    private static IServiceCollection SetSignalRTestSender(this IServiceCollection services, 
        TestServer signalRServer,
        string[] hubs,
        Func<IServiceCollection,string, HttpMessageHandler, IServiceCollection> addMessageSender)
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection)).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        services.RemoveImplementations<IMessageSender>("SignalR");        

        var handler = signalRServer.CreateHandler();

        foreach (var hub in hubs)
        {
            var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
            addMessageSender(services, signalRUrl, handler);
        }

        return services;
    }

    public static IServiceCollection SetSignalRSimpleTestSender(this IServiceCollection services, TestServer signalRServer, string[] hubs)
    {
        return services.SetSignalRTestSender(signalRServer, hubs, (services, url, handler) => services.AddSignalRMessageSender(url, handler));
    }

    public static IServiceCollection SetSignalRHubTestSender(this IServiceCollection services, TestServer signalRServer, string[] hubs)
    {
        return services.SetSignalRTestSender(signalRServer, hubs, (services, url, handler) => services.AddSignalRHubMessageSender(url, handler));
    }

    private static IServiceCollection SetSignalRTestSender(this IServiceCollection services, object key, TestServer signalRServer, string hub,
        Func<IServiceCollection, string, HttpMessageHandler, object, IServiceCollection> addKeyedMessageSender)
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection) &&
                    sd.IsKeyedService && sd.ServiceKey == key).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        services.RemoveKeyImplementations<IMessageSender>("SignalR", key);

        var handler = signalRServer.CreateHandler();
        var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
        addKeyedMessageSender(services, signalRUrl, handler, key);

        return services;
    }

    public static IServiceCollection SetSignalRSimpleTestSender(this IServiceCollection services, object key, TestServer signalRServer, string hub)
    {
        return services.SetSignalRTestSender(key, signalRServer, hub, (services, url, handler, key) => services.AddKeyedSignalRMessageSender(url, handler, key));
    }

    public static IServiceCollection SetSignalRHubTestSender(this IServiceCollection services, object key, TestServer signalRServer, string hub)
    {
        return services.SetSignalRTestSender(key, signalRServer, hub, (services, url, handler, key) => services.AddKeyedSignalRHubMessageSender(url, handler, key));
    }

    private static IServiceCollection SetSignalRTestReceiver(this IServiceCollection services, object key, TestServer signalRServer, string hub,
        Func<IServiceCollection, string, HttpMessageHandler, object, IServiceCollection> addMessageReceiver)
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection) &&
                    sd.IsKeyedService && sd.ServiceKey == key).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        services.RemoveKeyImplementations<IMessageReceiver>("SignalR", key);

        var handler = signalRServer.CreateHandler();
        var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";        
        addMessageReceiver(services, signalRUrl, handler, key);

        return services;
    }

    public static IServiceCollection SetSignalRSimpleTestReceiver(this IServiceCollection services, object key, TestServer signalRServer, string hub)
    {
        return services.SetSignalRTestReceiver(key, signalRServer, hub, (services, url, handler, key) => services.AddKeyedSignalRMessageReceiver(url, handler, key));
    }

    public static IServiceCollection SetSignalRHubTestReceiver(this IServiceCollection services, object key, TestServer signalRServer, string hub)
    {
        return services.SetSignalRTestReceiver(key, signalRServer, hub, (services, url, handler, key) => services.AddKeyedSignalRHubMessageReceiver(url, handler, key));
    }
}
