namespace Db.Infrastructure;

public class EntityWarningException : Exception
{
    public object? Result { get; }

    public EntityWarningException(object? result) : base() { Result = result; }

    public EntityWarningException(string message, object? result = null) : base(message) { Result = result; }

    public EntityWarningException(string? message, Exception? innerException, object? result = null) : base(message, innerException)
    {
        Result = result;
    }
}

public class EntityWarningException<T> : Exception
    where T:class, new()
{
    public T? Result { get; }

    public EntityWarningException(T? result) : base() { Result = result; }

    public EntityWarningException(string message, T? result) : base(message) { Result = result; }

    public EntityWarningException(string? message, Exception? innerException, T? result) : base(message, innerException)
    {
        Result = result;
    }
}