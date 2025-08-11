using Message.Interfaces;

namespace Alchemist.Test.EventBus.Interfaces;

public interface ITestHost : IDisposable, IAsyncDisposable
{
    Task Start();

    bool IsStarted { get; }

    Uri Uri { get; }

    IMessageSender CreatePublisher(params object[]? args);

    IMessageReceiver CreateSubscriber(params object[]? args);
}
