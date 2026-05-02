namespace Alchemist.Exceptions;

public class NotFoundException : Exception
{
    public IDictionary<string, object> ParamValues { get; } = new Dictionary<string,object>();

    public NotFoundException() : base() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, IDictionary<string, object> paramValues) : base(message) 
    { 
        ParamValues = paramValues; 
    }

    public NotFoundException(string? message, Exception? innerException, IDictionary<string, object> paramValues) : base(message, innerException)
    {
        ParamValues = paramValues;
    }
}
