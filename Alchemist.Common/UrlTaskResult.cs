namespace Alchemist.Common;

public readonly struct UrlTaskResult<T> 
{
    public ResultStatus Status { get; }

    public T? Value { get; }

    public Exception? Exception { get; }

    public UrlTaskResult() { Status = ResultStatus.Error; }

    public string Url { get; }

    private UrlTaskResult(T? value, string url, ResultStatus result, Exception? exception = null)
    {
        Status = result;
        Value = value;
        Exception = exception;
        Url = url;
    }

    public static UrlTaskResult<T> Success(T? value, string url)
    {
        return new UrlTaskResult<T>(value, url, ResultStatus.Success);
    }

    public static UrlTaskResult<T> Warning(T? value, string url, Exception? exception = null)
    {
        return new UrlTaskResult<T>(value, url, ResultStatus.Warning, exception);
    }

    public static UrlTaskResult<T> Failed(T? value, string url, Exception exception)
    {
        return new UrlTaskResult<T>(value, url, ResultStatus.Error, exception);
    }

    public static UrlTaskResult<T> Cancelled(string url)
    {
        return new UrlTaskResult<T>(default(T), url, ResultStatus.Cancelled, null);
    }
}
