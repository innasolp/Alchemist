namespace Mediator.Messages;

public interface IEventMessage
{
    (string messageFormat, object?[] args) GetSuccessEventMessage();

    (string messageFormat, object?[] args) GetFailedMessage();
}
