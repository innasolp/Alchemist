namespace Hangfire.AggregateJobs;

public class AggregateServerSettings
{
    public required string ServerName { get; set; }

    public string ChildServerName { get; set; } = "ChildServer";

    public required string WaitingQueue { get; set;  }

    public string ChildWaitingQueue { get; set; } = "waiting-children";

    public required string ProcessingQueue { get; set; }

    public string ChildProcessingQueue { get; set; } = "processing-children";

    public int ChildJobCountPerParent { get; set; } = 3;

    public int ParentWorkerCount { get; set; } = 10;
}