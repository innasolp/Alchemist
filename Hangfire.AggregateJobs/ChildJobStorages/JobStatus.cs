using System.Text.Json.Serialization;

namespace Hangfire.AggregateJobs.ChildJobStorages;

[JsonConverter(typeof(JsonStringEnumConverter<JobStatus>))]
public enum JobStatus
{
    Enqueued = 0,
    Processing = 1,
    Completed = 2,
    Deleted = 3,
    Failed = 4
}