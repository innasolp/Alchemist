namespace Alchemist.Common;

public enum Status
{
    Success = 0,
    Warning = 1, 
    Error = 2
}

public readonly struct TaskResult<T>
{
    public Status Status { get; }

    public T? Result { get; }

    public TaskResult() { Status = Status.Error; }

    private TaskResult(T? value, Status result)
    {
        Status = result;
        Result = value;
    }

    public static TaskResult<T> Success(T? value)
    {
        return new TaskResult<T>(value, Status.Success);
    }

    public static TaskResult<T> Warning(T? value)
    {
        return new TaskResult<T>(value, Status.Warning);
    }

    public static TaskResult<T> Failed(T? value)
    {
        return new TaskResult<T>(value, Status.Error);
    }    
}
