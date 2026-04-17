using Alchemist.Test.Log;
using Alchemist.Test.SignalRWebAppFactory;
using Message.Interfaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Shop.API.Test.Infrastructure;

internal static class SignalRCommon
{
    public static void ConfigureSignalRMock(this IServiceCollection services)
    {
        var signalRDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IMessageSender));
        if (signalRDescriptor != null)
            services.Remove(signalRDescriptor);

        var messageSenderMock = new Mock<IMessageSender>();
        messageSenderMock.Setup(s => s.Start(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        messageSenderMock.Setup(s => s.Send(It.IsAny<It.IsAnyType>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
         
        services.AddSingleton(messageSenderMock.Object);
    }
    

    private static SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRWebAppFactory;

    public static SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> SignalRWebAppFactory
    {
        get
        {
            _signalRWebAppFactory ??= new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
            return _signalRWebAppFactory;
        }
    }
}