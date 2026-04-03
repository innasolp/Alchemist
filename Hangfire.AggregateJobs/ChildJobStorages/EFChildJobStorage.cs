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

    public async Task DeleteParentJobAsync(string jobId)
    {
        await _dbContext.ParentJobEntries.Where(x => x.JobId == jobId).ExecuteDeleteAsync();
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots)
    {
        var childstatusEnqueued = 0;
        var parentstatusExecuting = 1;
        var childstatusExecuting = 1;
        var parentstatusEnqueued = 0;

        var sql = $@"WITH running_counts AS (
    -- Считаем реально исполняемые задачи
    SELECT 
        parent_job_id, 
        COUNT(*) as active_count
    FROM child_job_entry
    WHERE status = {childstatusExecuting}
    GROUP BY parent_job_id
),
active_parents AS (
    SELECT 
        p.job_id,
        p.created_at as parent_created_at,
        COALESCE(r.active_count, 0) as current_active
    FROM parent_job_entry p
    LEFT JOIN running_counts r ON p.job_id = r.parent_job_id
    WHERE p.status IN ({parentstatusExecuting}, {parentstatusEnqueued})
),
ranked_jobs AS (
    SELECT 
        c.*, 
        ap.parent_created_at,
        ap.current_active,
        -- Порядковый номер задачи В ЭТОЙ ВЫБОРКЕ для конкретного родителя
        ROW_NUMBER() OVER (
            PARTITION BY c.parent_job_id 
            ORDER BY c.created_at ASC
        ) as queue_pos
    FROM child_job_entry c
    INNER JOIN active_parents ap ON c.parent_job_id = ap.job_id
    WHERE c.status = {childstatusEnqueued}
)
SELECT 
    job_id, 
    parent_job_id, 
    status, 
    created_at
FROM ranked_jobs
WHERE 
    -- ЖЕСТКОЕ ОГРАНИЧЕНИЕ: не более лимита задач на одного родителя за этот запрос
    queue_pos <= {childJobCountPerParent}
ORDER BY 
    -- 1. Сначала заполняем ""забронированные"" слоты (те, кто еще не исчерпал общий лимит)
    CASE WHEN (current_active + queue_pos) <= {childJobCountPerParent} THEN 0 ELSE 1 END ASC,
    
    -- 2. Приоритет родителям, у которых сейчас меньше всего реально запущенных задач
    current_active ASC,
    
    -- 3. ПЕРЕМЕШИВАНИЕ (Round Robin): берем по одной задаче от каждого родителя по очереди
    -- Это исключает ситуацию, когда старый родитель забирает весь freeSlots
    queue_pos ASC,
    
    -- 4. Если всё остальное равно — по дате создания родителя
    parent_created_at ASC
LIMIT {freeSlots}";

        return await _dbContext.ChildJobEntries
                .FromSqlRaw(sql)
                .Select(j => j.JobId)
            .AsNoTracking()
            .ToListAsync();
    }

    public string? GetParentJobId(string jobId)
    {
       var entry = _dbContext.ChildJobEntries.FirstOrDefault(c=>c.JobId ==  jobId);
        return entry?.ParentJobId;
    }

    public async Task UpdateChildJobsStateAsync(IEnumerable<string> jobIds, JobStatus state)
    {
        await _dbContext.ChildJobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, state));

        await _dbContext.SaveChangesAsync();
    }

    public void UpdateChildJobState(string jobId, JobStatus state)
    {
        _dbContext.ChildJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state));
        
        _dbContext.SaveChanges();
    }

    public void UpdateParentJobState(string jobId, JobStatus state, DateTime updateAt)
    {
        _dbContext.ParentJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state).SetProperty(b=>b.UpdatedAt, updateAt));

        _dbContext.SaveChanges();
    }

    public async Task UpdateParentJobStateAsync(string jobId, JobStatus state, DateTime updateAt)
    {
        await _dbContext.ParentJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, state).SetProperty(b => b.UpdatedAt, updateAt));

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateParentJobIdAsync(IEnumerable<string> jobIds, string parentJobId)
    {
        await _dbContext.ChildJobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.ParentJobId, parentJobId));

        await _dbContext.SaveChangesAsync();
    }

    public Task<ParentJobEntry?> GetParentJobAsync(string jobId)
    {
        return _dbContext.ParentJobEntries.Where(x => x.JobId == jobId).FirstOrDefaultAsync();
    }

    public void UpdateParentJobDate(string jobId, DateTime updateAt)
    {
        _dbContext.ParentJobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.UpdatedAt, updateAt));

        _dbContext.SaveChanges();
    }

    public async Task CreateParentJobIdleSettingsAsync(ParentJobIdleSettings parentJobIdleSettings)
    {
        await _dbContext.AddAsync(parentJobIdleSettings);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetIdleParentJobsIds(DateTime currentDate, CancellationToken cancellationToken = default)
    {
        var query = from parent in _dbContext.ParentJobEntries
                    join settings in _dbContext.ParentJobIdleSettings on parent.JobId equals settings.JobId
                    where parent.Status == JobStatus.Processing
                    where parent.UpdatedAt < currentDate.AddSeconds(-settings.IdleTimeInSeconds)
                    where !_dbContext.ChildJobEntries.Any(c =>
                        c.ParentJobId == parent.JobId &&
                        (c.Status == JobStatus.Enqueued || c.Status == JobStatus.Processing))
                    select parent.JobId;

        return await query.ToListAsync(cancellationToken);
    }
}