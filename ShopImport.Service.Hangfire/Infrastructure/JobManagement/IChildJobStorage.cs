namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal interface IChildJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots);

    Task UpdateJobsStateAsync(IEnumerable<string> jobIds, int state);

    void UpdateJobsState(IEnumerable<string> jobIds, int state);

    void UpdateJobState(string jobId, int state);

    string? GetParentJobId(string jobId);

    Task CreateChildJobEntryAsync(ChildJobEntry childJobEntry);
}