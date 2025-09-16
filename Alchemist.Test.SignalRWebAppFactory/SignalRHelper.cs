using Message.Interfaces;
using Message.SignalR;
using Message.SignalR.DependencyInjection;
using Message.SignalR.HubMessage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Test.SignalRWebAppFactory;

public class SignalRHelper
{
    private static T CreateSignalRMessageProcessor<T>(IServiceProvider serviceProvider, TestServer signalRServer, string hub,
        Func<ILogger<T>, HubConnection, T> getMessageProcessor)
        where T: IMessageProcessor
    { 
        var handler = signalRServer.CreateHandler();
        var signalRUrl = $"{signalRServer.BaseAddress.AbsoluteUri}{hub}";
        var hubConnection = new HubConnectionBuilder().CreateHubConnection(signalRUrl,
            handler,
                    logging =>
                    {
                        //logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    });

        var logger = serviceProvider.GetRequiredService<ILogger<T>>();

        return getMessageProcessor(logger, hubConnection); //new SignalRMessageSender(logger, hubConnection);
    }

    public static SignalRMessageSender CreateTestSignalRMessageSender(IServiceProvider serviceProvider, TestServer signalRServer, string hub)
    {
        return CreateSignalRMessageProcessor<SignalRMessageSender>(serviceProvider, signalRServer, hub,
            (logger, hubConnection) => new SignalRMessageSender(logger, hubConnection));
    }

    public static SignalRMessageHubSender CreateTestSignalRMessageHubSender(IServiceProvider serviceProvider, TestServer signalRServer, string hub)
    {
        return CreateSignalRMessageProcessor<SignalRMessageHubSender>(serviceProvider, signalRServer, hub,
            (logger, hubConnection) => new SignalRMessageHubSender(logger, hubConnection));
    }

    public static SignalRMessageReceiver CreateTestSignalRMessageReceiver(IServiceProvider serviceProvider, TestServer signalRServer, string hub)
    {
        return CreateSignalRMessageProcessor<SignalRMessageReceiver>(serviceProvider, signalRServer, hub,
            (logger, hubConnection) => new SignalRMessageReceiver(logger, hubConnection));
    }

    public static SignalRMessageHubReceiver CreateTestSignalRMessageHubReceiver(IServiceProvider serviceProvider, TestServer signalRServer, string hub)
    {
        return CreateSignalRMessageProcessor<SignalRMessageHubReceiver>(serviceProvider, signalRServer, hub,
            (logger, hubConnection) => new SignalRMessageHubReceiver(logger, hubConnection));
    }    
}
