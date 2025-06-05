namespace Alchemist.Common;

public enum Status
{
    Success = 0,
    Warning = 1,
    Error = 2,
    Cancelled = 3
}

public readonly struct TaskResult
{
    public Status Status { get; }

    public Exception? Exception { get; }

    public TaskResult() { Status = Status.Error; }

    private TaskResult(Status result, Exception? exception = null)
    {
        Status = result;
        Exception = exception;
    }

    public static TaskResult Success()
    {
        return new TaskResult(Status.Success);
    }

    public static TaskResult Warning(Exception? exception = null)
    {
        return new TaskResult(Status.Warning, exception);
    }

    public static TaskResult Failed(Exception exception)
    {
        return new TaskResult(Status.Error, exception);
    }

    public static TaskResult Cancelled()
    {
        return new TaskResult(Status.Cancelled, null);
    }
}

public readonly struct TaskResult<T>
{
    public Status Status { get; }

    public T? Result { get; }

    public Exception? Exception { get; }

    public TaskResult() { Status = Status.Error; }

    private TaskResult(T? value, Status result, Exception? exception = null)
    {
        Status = result;
        Result = value;
        Exception = exception;
    }

    public static TaskResult<T> Success(T? value)
    {
        return new TaskResult<T>(value, Status.Success);
    }

    public static TaskResult<T> Warning(T? value, Exception? exception = null)
    {
        return new TaskResult<T>(value, Status.Warning, exception);
    }

    public static TaskResult<T> Failed(T? value, Exception exception)
    {
        return new TaskResult<T>(value, Status.Error, exception);
    }
    
    public static TaskResult<T> Cancelled()
    {
        return new TaskResult<T>(default(T), Status.Cancelled, null);
    }
}
