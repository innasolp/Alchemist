using Hangfire.AggregateJobs.ChildJobStorages;

namespace Hangfire.AggregateJobs;

public interface IAggregateJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots);

    Task UpdateJobsStateAsync(IEnumerable<string> jobIds, JobStatus state);

    string? GetParentJobId(string jobId);

    Task CreateJobEntryAsync(JobEntry childJobEntry);

    Task DeleteJobAsync(string jobId);

    Task<JobEntry?> GetJobAsync(string jobId);    

    void UpdateJobState(string jobId, JobStatus state, DateTime updateAt);

    Task UpdateJobStateAsync(string jobId, JobStatus state, DateTime updateAt);

    bool JobExists(string jobId);

    Task UpdateParentJobIdAsync(IEnumerable<string> jobIds, string parentJobId);

    Task CreateParentJobIdleSettingsAsync(ParentJobIdleSettings parentJobIdleSettings);

    Task<IEnumerable<string>> GetIdleParentJobsIds(DateTime currentDate, CancellationToken cancellationToken = default);
}