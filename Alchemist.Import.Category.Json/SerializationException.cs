namespace Alchemist.Import.Category.Service;

public class SerializationException : Exception
{
    public SerializationException()
    {
    }

    public SerializationException(string? message) : base(message)
    {
    }

    public SerializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}