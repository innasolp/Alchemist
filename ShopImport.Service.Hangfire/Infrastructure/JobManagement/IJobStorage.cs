namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal interface IJobStorage
{
    Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots);

    Task UpdateJobsStateAsync(IEnumerable<string> jobIds, int state);

    void UpdateJobsState(IEnumerable<string> jobIds, int state);

    void UpdateJobState(string jobId, int state);

    string? GetParentJobId(string jobId);
}