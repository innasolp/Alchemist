using MediatR;

namespace Mediator.Infrastructure;

public interface IEvent : INotification
{
    DateTime CreationDate { get; }

    string EventName { get; }
}