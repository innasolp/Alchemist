namespace Alchemist.Import.Interfaces;

public class ImportCanceledException : Exception
{
    public ImportCanceledException()
    {
    }

    public ImportCanceledException(string? message) : base(message)
    {
    }

    public ImportCanceledException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
