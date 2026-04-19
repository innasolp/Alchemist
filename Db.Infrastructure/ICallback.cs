namespace Db.Infrastructure;

public class EventArgs<T>(T value) : EventArgs
{
    public T Value { get; } = value;
}

public interface ICallback<T>    
{
    event EventHandler<EventArgs<T>> Callback; 
}