using System.ComponentModel.DataAnnotations;

namespace Hangfire.AggregateJobs.ChildJobStorages;

public class JobEntry
{
    [Key]
    public required string JobId { get; set; }

    public string? ParentJobId { get; set; }

    public string? ExecutionId { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Enqueued;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}