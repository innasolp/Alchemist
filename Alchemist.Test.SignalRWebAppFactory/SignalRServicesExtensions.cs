using Message.Interfaces;
using Message.SignalR.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.SignalRWebAppFactory;

public static class SignalRServicesExtensions
{
    public static IServiceCollection SetSignalRTestSender(this IServiceCollection services, TestServer signalRServer, string[] hubs )
    {
        var hubConnectionDescriptors = services.Where(sd => sd.ServiceType == typeof(HubConnection)).ToList();
        hubConnectionDescriptors.ForEach(d => services.Remove(d));

        var messageSenderDescriptors = services.Where(sd => sd.ServiceType == typeof(IMessageSender)).ToList();
        messageSenderDescriptors.ForEach(d => services.Remove(d));

        var handler = signalRServer.CreateHandler();

        foreach (var hub in hubs)
        {
            var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
            services.AddSignalRMessageSender(signalRUrl, handler);
        }

        return services;
    }
}
