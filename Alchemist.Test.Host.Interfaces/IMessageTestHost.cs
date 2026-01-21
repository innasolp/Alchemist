using Message.Interfaces;

namespace Alchemist.Test.Host.Interfaces;

public interface IMessageTestHost : IDisposable, IAsyncDisposable
{
    Task Start();

    bool IsStarted { get; }

    Uri Uri { get; }

    IMessageSender CreatePublisher(params object[]? args);

    IMessageReceiver CreateSubscriber(params object[]? args);
}
