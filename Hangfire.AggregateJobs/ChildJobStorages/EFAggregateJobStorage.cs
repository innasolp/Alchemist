using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Immutable;

namespace Hangfire.AggregateJobs.ChildJobStorages;

internal class EFAggregateJobStorage(AggregateJobDbContext dbContext) : IAggregateJobStorage
{
    private readonly AggregateJobDbContext _dbContext = dbContext;

    private IDbContextTransaction? _currentTransaction;

    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);

                DisposeTransaction();
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            DisposeTransaction();
        }
    }

    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            DisposeTransaction();
        }
    }

    public void Dispose()
    {
        DisposeTransaction();
    }

    private void DisposeTransaction()
    {
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }

    public async Task CreateJobEntryAsync(JobEntry childJobEntry, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(childJobEntry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteJobEntryAsync(string jobId, CancellationToken cancellationToken = default)
    {
        await _dbContext.JobEntries.Where(x => x.JobId == jobId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetChildJobIdsForProcessing(int childJobCountPerParent, int freeSlots,
        CancellationToken cancellationToken = default)
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
    FROM job_entry
    WHERE status = {childstatusExecuting} and parent_job_id is not null
    GROUP BY parent_job_id
),
active_parents AS (
    SELECT 
        p.job_id,
        p.created_at as parent_created_at,
        COALESCE(r.active_count, 0) as current_active
    FROM job_entry p
    LEFT JOIN running_counts r ON p.job_id = r.parent_job_id
    WHERE p.parent_job_id is null and p.status IN ({parentstatusExecuting}, {parentstatusEnqueued})
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
    FROM job_entry c
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

        return await _dbContext.JobEntries
                .FromSqlRaw(sql)
                .Select(j => j.JobId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void UpdateJobEntryState(string jobId, JobStatus state, DateTime updateAt)
    {
        _dbContext.JobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdate(s => s.SetProperty(b => b.Status, state).SetProperty(b=>b.UpdatedAt, updateAt));

        _dbContext.SaveChanges();
    }

    public async Task UpdateJobEntryStateAsync(string jobId, JobStatus state, DateTime updateAt, CancellationToken cancellationToken = default)
    {
        await _dbContext.JobEntries
           .Where(j => j.JobId == jobId)
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, state).SetProperty(b => b.UpdatedAt, updateAt), cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateParentJobIdAsync(IEnumerable<string> jobIds, string parentJobId, CancellationToken cancellationToken = default)
    {
        await _dbContext.JobEntries
           .Where(j => jobIds.Contains(j.JobId))
           .ExecuteUpdateAsync(s => s.SetProperty(b => b.ParentJobId, parentJobId), cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateParentJobIdleSettingsAsync(ParentJobIdleSettings parentJobIdleSettings, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(parentJobIdleSettings, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetIdleParentJobsIdsAsync(DateTime currentDate, CancellationToken cancellationToken = default)
    {
        var query = from parent in _dbContext.JobEntries
                    join settings in _dbContext.ParentJobIdleSettings on parent.JobId equals settings.JobId
                    where parent.Status == JobStatus.Processing
                    where parent.UpdatedAt < currentDate.AddSeconds(-settings.IdleTimeInSeconds)
                    where !_dbContext.JobEntries.Any(c =>
                        c.ParentJobId == parent.JobId &&
                        (c.Status == JobStatus.Enqueued || c.Status == JobStatus.Processing))
                    select parent.JobId;

        return await query.ToListAsync(cancellationToken);
    }

    public Task<JobEntry?> GetJobEntryByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.JobEntries.Where(x => x.ExecutionId == executionId).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> JobExecutionExistsAsync(string executionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.JobEntries.AnyAsync(x => x.ExecutionId == executionId, cancellationToken);
    }

    public async Task<IEnumerable<(string ExecutionId, string JobId)>>
        GetJobIdsByExecutionIdsAsync(IEnumerable<string> executionIds, CancellationToken cancellationToken = default)
    {
        var jobs = await _dbContext.JobEntries
           .Where(j => executionIds.Contains(j.ExecutionId))
           .Select(j => new { j.ExecutionId, j.JobId })
           .ToListAsync(cancellationToken);

        return jobs.Select( j=> (j.ExecutionId!, j.JobId));
    }

    public JobEntry? GetJobEntryByExecutionId(string executionId)
    {
        return _dbContext.JobEntries.Where(x => x.ExecutionId == executionId).FirstOrDefault();
    }

    public async Task DeleteJobEntryByExecutionIdAsync(string executionId, CancellationToken cancellationToken = default)
    {
        await _dbContext.JobEntries.Where(x => x.ExecutionId == executionId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetExpiredJobIdsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.JobEntries
           .Where(j => j.Status == JobStatus.Completed || j.Status == JobStatus.Deleted || j.Status == JobStatus.Failed )
           .Select(j => j.JobId )
           .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetChildJobIdsAsync(string parentJobId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JobEntries
           .Where(j => j.ParentJobId == parentJobId)
           .Select(j => j.JobId)
           .ToListAsync(cancellationToken);
    }
}