namespace ShopImport.Service.Hangfire.Infrastructure;

public class JobExecuteOptions
{
    public required string ServerName { get; set; }

    public required string WaitingQueue { get; set;  }

    public required string ProcessingQueue { get; set; }

    public int? EnqueuedInSeconds { get; set; } = null;

    public int? IntervalInSeconds { get; set; } = null;
}