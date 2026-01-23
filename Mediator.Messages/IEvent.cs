using MediatR;

namespace Mediator.Messages;

public interface IEvent : INotification
{
    DateTime CreationDate { get; }

    string EventName { get; }
}