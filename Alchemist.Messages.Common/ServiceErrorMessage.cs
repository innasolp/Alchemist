namespace Alchemist.Messages.Common;

public class ServiceErrorMessage : ServiceMessage
{
    public string EventName { get; set; }

    public string? Message { get; set; }

    public Exception? Exception { get; set; }
}
