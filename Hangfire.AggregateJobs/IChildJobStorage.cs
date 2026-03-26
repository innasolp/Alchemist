namespace Hangfire.AggregateJobs;

internal interface IChildJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots);

    Task UpdateJobsStateAsync(IEnumerable<string> jobIds, JobStatus state);

    void UpdateJobsState(IEnumerable<string> jobIds, JobStatus state);

    void UpdateJobState(string jobId, JobStatus state);

    string? GetParentJobId(string jobId);

    Task CreateChildJobEntryAsync(ChildJobEntry childJobEntry);

    Task CreateParentJobEntryAsync(ParentJobEntry parentJobEntry);

    void DeleteParentJob(string jobId);

    void UpdateParentJobState(string jobId, JobStatus state);

    bool ParentJobExists(string jobId);
}