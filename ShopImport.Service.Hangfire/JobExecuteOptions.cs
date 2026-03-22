namespace ShopImport.Service.Hangfire;

public class JobExecuteOptions
{
    public required string ServerName { get; set; }

    public string? ChildServerName { get; set; }

    public required string WaitingQueue { get; set;  }

    public string? ChildWaitingQueue { get; set; }

    public required string ProcessingQueue { get; set; }

    public string? ChildProcessingQueue { get; set; }

    public int ChildJobCountPerParent { get; set; } = 3;
}