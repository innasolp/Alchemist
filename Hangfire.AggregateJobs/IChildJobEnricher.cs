using Hangfire.Storage;

namespace Hangfire.AggregateJobs;

public interface IChildJobEnricher<T>
{
    void Enrich(string jobId, T job, T parentJob);
}