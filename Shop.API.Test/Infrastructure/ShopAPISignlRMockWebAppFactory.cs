using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Shop.API.Test.Infrastructure;

public class ShopAPISignlRMockWebAppFactory : ShopAPIWebAppFactory
{ 
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var signalRDescriptor = services.SingleOrDefault(s=>s.ServiceType == typeof(IMessageSender));
        if (signalRDescriptor != null)
            services.Remove(signalRDescriptor);

        var messageSenderMock = new Mock<IMessageSender>();
        messageSenderMock.Setup(s => s.Start(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        messageSenderMock.Setup(s => s.Send(It.IsAny<It.IsAnyType>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        services.AddSingleton(messageSenderMock.Object);
    }

    protected override void ConfigureWebHostBuilderContext(Microsoft.AspNetCore.Hosting.WebHostBuilderContext context, IServiceCollection services)
    {        
    }
}
