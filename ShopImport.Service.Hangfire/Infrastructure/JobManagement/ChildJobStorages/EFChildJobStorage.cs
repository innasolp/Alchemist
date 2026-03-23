using Microsoft.EntityFrameworkCore;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement.ChildJobStorages;

internal class EFChildJobStorage(ChildJobDbContext dbContext) : IChildJobStorage
{
    private readonly ChildJobDbContext _dbContext = dbContext;

    public async Task CreateChildJobEntryAsync(ChildJobEntry childJobEntry)
    {
        await _dbContext.AddAsync(childJobEntry).AsTask();
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots)
    {
        return await _dbContext.ChildJobEntries
                    .Where(j => j.Status == JobStatus.Enqueued)
                    .Select(j => j.ParentJobId).Distinct()                    
                    .SelectMany(parentId => _dbContext.ChildJobEntries
                        .Where(child => child.ParentJobId == parentId && child.Status == JobStatus.Enqueued)
                        .OrderBy(child => child.CreatedAt)
                        .Take(childJobCountPerParent)
                    )
                    .Take(freeSlots)
                    .Select(j => j.JobId)
                    .ToListAsync(); ;
    }

    public string? GetParentJobId(string jobId)
    {
       var entry = _dbContext.ChildJobEntries.FirstOrDefault(c=>c.JobId ==  jobId);
        return entry?.ParentJobId;
    }

    public void UpdateJobsState(IEnumerable<string> jobIds, JobStatus state)
    {
        _dbContext.ChildJobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state));

        _dbContext.SaveChanges();
    }

    public async Task UpdateJobsStateAsync(IEnumerable<string> jobIds, JobStatus state)
    {
        await _dbContext.ChildJobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, state));

        await _dbContext.SaveChangesAsync();
    }

    public void UpdateJobState(string jobId, JobStatus state)
    {
        _dbContext.ChildJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state));
        
        _dbContext.SaveChanges();
    }
}