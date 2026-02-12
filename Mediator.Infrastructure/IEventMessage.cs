namespace Mediator.Infrastructure;

public interface IEventMessage
{
    (string messageFormat, object?[] args) GetSuccessEventMessage();

    (string messageFormat, object?[] args) GetFailedMessage();
}
