using Hangfire.AggregateJobs.ChildJobStorages;

namespace Hangfire.AggregateJobs;

public interface IAggregateJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots);

    Task UpdateJobsStateAsync(IEnumerable<string> jobIds, JobStatus state);

    Task CreateJobEntryAsync(JobEntry childJobEntry);

    Task DeleteJobAsync(string jobId);  
    
    Task<JobEntry?> GetJobByExecutionIdAsync(string executionId);    

    JobEntry? GetJobByExecutionId(string executionId);    

    void UpdateJobState(string jobId, JobStatus state, DateTime updateAt);

    Task UpdateJobStateAsync(string jobId, JobStatus state, DateTime updateAt);

    Task<bool> JobExecutionExistsAsync(string executionId);

    Task UpdateParentJobIdAsync(IEnumerable<string> jobIds, string parentJobId);

    Task CreateParentJobIdleSettingsAsync(ParentJobIdleSettings parentJobIdleSettings);

    Task<IEnumerable<string>> GetIdleParentJobsIdsAsync(DateTime currentDate, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetJobIdsByExecutionIdsAsync(IEnumerable<string> executionIds);
}