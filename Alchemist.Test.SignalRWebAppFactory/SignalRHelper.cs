using Message.Interfaces;
using Message.SignalR;
using Message.SignalR.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Test.SignalRWebAppFactory;

public class SignalRHelper
{
    public static IMessageSender CreateSignalRTestSender(IServiceProvider serviceProvider, TestServer signalRServer, string hub)
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

        var logger = serviceProvider.GetRequiredService<ILogger<SignalRMessageSender>>();

        return new SignalRMessageSender(logger, hubConnection);
    }

    public static IMessageReceiver CreateSignalRTestReceiver(IServiceProvider serviceProvider, TestServer signalRServer, string hub)
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

        var logger = serviceProvider.GetRequiredService<ILogger<SignalRMessageReceiver>>();

        return new SignalRMessageReceiver(logger, hubConnection);
    }
}
