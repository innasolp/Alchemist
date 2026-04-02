using Hangfire.AggregateJobs.ChildJobStorages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hangfire.AggregateJobs;

public interface IChildJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots);

    Task UpdateChildJobsStateAsync(IEnumerable<string> jobIds, JobStatus state);

    void UpdateChildJobState(string jobId, JobStatus state);

    string? GetParentJobId(string jobId);

    Task<ParentJobEntry?> GetParentJobAsync(string jobId);

    Task CreateChildJobEntryAsync(ChildJobEntry childJobEntry);

    Task CreateParentJobEntryAsync(ParentJobEntry parentJobEntry);

    Task DeleteParentJobAsync(string jobId);

    void UpdateParentJobState(string jobId, JobStatus state, DateTime updateAt);

    Task UpdateParentJobStateAsync(string jobId, JobStatus state, DateTime updateAt);

    Task UpdateParentJobDateAsync(string jobId, DateTime updateAt);

    bool ParentJobExists(string jobId);

    Task UpdateParentJobIdAsync(IEnumerable<string> jobIds, string parentJobId);

    Task CreateParentJobIdleSettingsAsync(ParentJobIdleSettings parentJobIdleSettings);

    Task<IEnumerable<string>> GetIdleParentJobsIds(DateTime currentDate, CancellationToken cancellationToken = default);
}