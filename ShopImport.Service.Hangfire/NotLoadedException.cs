namespace ShopImport.Service.Hangfire;

internal class NotLoadedException : Exception
{
    public NotLoadedException()
    {
    }

    public NotLoadedException(string? message) : base(message)
    {
    }

    public NotLoadedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}