namespace Mediator.Messages;

public interface IEventMessage
{
    protected internal (string messageFormat, object?[] args) GetSuccessEventMessage();

    protected internal (string messageFormat, object?[] args) GetFailedMessage();
}
