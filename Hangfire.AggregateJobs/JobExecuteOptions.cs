namespace Hangfire.AggregateJobs;

public class JobExecuteOptions
{
    public int? EnqueuedInSeconds { get; set; } = null;

    public int? IntervalInSeconds { get; set; } = null;
}
