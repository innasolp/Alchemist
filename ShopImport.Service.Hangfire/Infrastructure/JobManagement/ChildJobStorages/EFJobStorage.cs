using Microsoft.EntityFrameworkCore;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.ChildJobStorages;

internal class EFJobStorage(JobDbContext dbContext) : IJobStorage
{
    private readonly JobDbContext _dbContext = dbContext;

    public async Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots)
    {
        return await _dbContext.ChildJobEntries
            .Where(j => j.Status == 0)
            .OrderBy(j => j.CreatedAt) // Сначала старые родители
            .GroupBy(j => j.ParentJobId)
            .SelectMany(g => g.OrderBy(j => j.CreatedAt).Take(childJobCountPerParent))
            .Take(freeSlots)
            .Select(j => j.JobId)
            .ToListAsync();
    }

    public string? GetParentJobId(string jobId)
    {
       var entry = _dbContext.ChildJobEntries.FirstOrDefault(c=>c.JobId ==  jobId);
        return entry?.ParentJobId;
    }

    public void UpdateJobsState(IEnumerable<string> jobIds, int state)
    {
        _dbContext.ChildJobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state));
    }

    public Task UpdateJobsStateAsync(IEnumerable<string> jobIds, int state)
    {
        return _dbContext.ChildJobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, state));
    }

    public void UpdateJobState(string jobId, int state)
    {
        _dbContext.ChildJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state));
    }
}