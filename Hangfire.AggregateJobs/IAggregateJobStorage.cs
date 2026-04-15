using Hangfire.AggregateJobs.ChildJobStorages;

namespace Hangfire.AggregateJobs;

public interface IAggregateJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots, CancellationToken cancellationToken = default);

    Task UpdateJobsStateAsync(IEnumerable<string> jobIds, JobStatus state, CancellationToken cancellationToken = default);

    Task CreateJobEntryAsync(JobEntry childJobEntry, CancellationToken cancellationToken = default);

    Task DeleteJobEntryAsync(string jobId, CancellationToken cancellationToken = default);  

    Task DeleteJobEntryByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default);  
    
    Task<JobEntry?> GetJobEntryByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default);    

    JobEntry? GetJobEntryByExecutionId(string executionId);    

    void UpdateJobEntryState(string jobId, JobStatus state, DateTime updateAt);

    Task UpdateJobEntryStateAsync(string jobId, JobStatus state, DateTime updateAt, CancellationToken cancellationToken = default);

    Task<bool> JobExecutionExistsAsync(string executionId, CancellationToken cancellationToken = default);

    Task UpdateParentJobIdAsync(IEnumerable<string> jobIds, string parentJobId, CancellationToken cancellationToken = default);

    Task CreateParentJobIdleSettingsAsync(ParentJobIdleSettings parentJobIdleSettings, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetIdleParentJobsIdsAsync(DateTime currentDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<(string ExecutionId, string JobId)>> GetJobIdsByExecutionIdsAsync(IEnumerable<string> executionIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetExpiredJobIdsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetChildJobIdsAsync(string parentJobId, CancellationToken cancellationToken = default);
}