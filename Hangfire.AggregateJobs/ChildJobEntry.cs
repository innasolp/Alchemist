using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hangfire.AggregateJobs;

[JsonConverter(typeof(JsonStringEnumConverter<JobStatus>))]
public enum JobStatus
{
    Enqueued = 0,
    Processing = 1,
    Completed = 2,
    Deleted = 3
}

internal class ChildJobEntry
{
    [Key]
    public required string JobId { get; set; }
    public required string ParentJobId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Enqueued;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime ParentCreatedAt { get; set; }
}