using Microsoft.EntityFrameworkCore;

namespace Hangfire.AggregateJobs.ChildJobStorages;

internal class EFChildJobStorage(ChildJobDbContext dbContext) : IChildJobStorage
{
    private readonly ChildJobDbContext _dbContext = dbContext;

    public bool ParentJobExists(string jobId)
    {
        return _dbContext.ParentJobEntries.Any(x=>x.JobId ==  jobId);
    }

    public async Task CreateChildJobEntryAsync(ChildJobEntry childJobEntry)
    {
        await _dbContext.AddAsync(childJobEntry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateParentJobEntryAsync(ParentJobEntry parentJobEntry)
    {
        await _dbContext.AddAsync(parentJobEntry);
        await _dbContext.SaveChangesAsync();
    }

    public void DeleteParentJob(string jobId)
    {
        _dbContext.ParentJobEntries.Where(x => x.JobId == jobId).ExecuteDelete();
    }

    public async Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots)
    {
        var childstatusEnqueued = 0;
        var parentstatusExecuting = 1;

        var sql = $@"WITH running_counts AS (
                SELECT 
                    parent_job_id, 
                    COUNT(*) as active_count
                FROM child_job_entry
                WHERE status = {childstatusEnqueued}
                GROUP BY parent_job_id
            ),
            ranked_jobs AS (
                SELECT 
                    c.*, 
                    p.created_at as parent_created_at,
                    ROW_NUMBER() OVER (
                        PARTITION BY c.parent_job_id 
                        ORDER BY c.created_at ASC
                    ) + COALESCE(r.active_count, 0) as adjusted_rank
                FROM child_job_entry c
                INNER JOIN parent_job_entry p ON c.parent_job_id = p.job_id
                LEFT JOIN running_counts r ON c.parent_job_id = r.parent_job_id
                WHERE c.status = {childstatusEnqueued} 
                  AND p.status = {parentstatusExecuting}
            )
            -- 3. Финальная сортировка и выборка
            SELECT 
                job_id, 
                parent_job_id, 
                status, 
                created_at
            FROM ranked_jobs
            ORDER BY 
                CASE WHEN adjusted_rank <= {childJobCountPerParent} THEN 0 ELSE 1 END ASC,
                parent_created_at ASC,
                created_at ASC
            LIMIT {freeSlots}";

       return await _dbContext.ChildJobEntries
            .FromSqlRaw(sql)
            .Select(j => j.JobId)
            .ToListAsync();
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

    public void UpdateParentJobState(string jobId, JobStatus state)
    {
        _dbContext.ParentJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state));

        _dbContext.SaveChanges();
    }
}