using Alchemist.Test.EventBus.Interfaces;
using Message.Interfaces;
using Message.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.RabbitMq;

namespace Alchemist.Test.RabbitMQ;

public class RabbitMQTestHost() : ITestHost
{
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder().Build();    

    public bool IsStarted => _rabbitMqContainer.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running;

    public Uri Uri => new(_rabbitMqContainer.GetConnectionString());

    public IMessageSender CreatePublisher(params object[]? args)
    {
        if(args?.FirstOrDefault() is not IServiceProvider services)
            throw new InvalidOperationException("Parameter IServiceProvider is required.");

        if (args.Length < 2 || args[1] is not string exchangeName)
            throw new InvalidOperationException("Parameter exchangeName is required.");        

        var logger = services.GetRequiredService<ILogger<RabbitMQPublisher>>();

        return new RabbitMQPublisher(logger, Uri, exchangeName);
    }

    public IMessageReceiver CreateSubscriber(params object[]? args)
    {
        if (args?.FirstOrDefault() is not IServiceProvider services)
            throw new InvalidOperationException("Parameter IServiceProvider is required.");

        if (args.Length < 2 || args[1] is not string exchangeName)
            throw new InvalidOperationException("Parameter exchangeName is required.");

        if (args.Length < 3 || args[2] is not string queueName)        
            throw new InvalidOperationException("Parameter queueName is required.");

        var logger = services.GetRequiredService<ILogger<RabbitMQMessageReceiver>>();

        return new RabbitMQMessageReceiver(logger, Uri, exchangeName, queueName);
    }

    public void Dispose()
    {
        var joinableTaskFactory = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(new Microsoft.VisualStudio.Threading.JoinableTaskContext());
        joinableTaskFactory.Run(async () =>
        {
            await _rabbitMqContainer.StopAsync();
            await _rabbitMqContainer.DisposeAsync();
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
        await _rabbitMqContainer.DisposeAsync();
    }

    public async Task Start()
    {
        if(_rabbitMqContainer.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
            await _rabbitMqContainer.StartAsync().ConfigureAwait(false);
    }
}
