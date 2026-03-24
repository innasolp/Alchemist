namespace ShopImport.Service.Hangfire;

internal class JobDeletedException : Exception
{
    public JobDeletedException()
    {
    }

    public JobDeletedException(string? message) : base(message)
    {
    }

    public JobDeletedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}