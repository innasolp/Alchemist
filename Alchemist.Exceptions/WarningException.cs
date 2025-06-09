namespace Alchemist.Exceptions;

public class WarningException : Exception
{
    public object? Result { get; }

    public WarningException(object? result) : base() { Result = result; }

    public WarningException(string message, object? result) : base(message) { Result = result; }

    public WarningException(string? message, Exception? innerException, object? result) : base(message, innerException)
    {
        Result = result;
    }
}

public class WarningException<T> : Exception
    where T:class, new()
{
    public T? Result { get; }

    public WarningException(T? result) : base() { Result = result; }

    public WarningException(string message, T? result) : base(message) { Result = result; }

    public WarningException(string? message, Exception? innerException, T? result) : base(message, innerException)
    {
        Result = result;
    }
}
