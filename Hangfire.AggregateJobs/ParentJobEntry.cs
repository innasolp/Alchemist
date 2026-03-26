using System.ComponentModel.DataAnnotations;

namespace Hangfire.AggregateJobs;

public class ParentJobEntry
{
    [Key]
    public required string JobId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public JobStatus Status { get; set; } = JobStatus.Enqueued;
}