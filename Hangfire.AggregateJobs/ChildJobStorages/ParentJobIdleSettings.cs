using System.ComponentModel.DataAnnotations;

namespace Hangfire.AggregateJobs.ChildJobStorages;

public class ParentJobIdleSettings
{
    [Key]
    public required string JobId { get; set; }

    public int IdleTimeInSeconds { get; set; }
}