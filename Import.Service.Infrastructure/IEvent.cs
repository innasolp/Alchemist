using MediatR;

namespace Import.Service.Commands;

public interface IEvent : INotification
{
    DateTime CreationDate { get; }

    string EventName { get; }
}

public interface IEvent<T> : IEvent
{ 
    T Message { get; }
}