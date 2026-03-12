using Hangfire;
using Hangfire.States;
using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal abstract class ImportServiceJob(IJobExecutor jobExecutor, IBackgroundJobClient backgroundJobClient
    Guid? parentId = null) : IImportServiceJob
{
    private readonly IJobExecutor _jobExecutor = jobExecutor;

    protected IBackgroundJobClient BackgroundJobClient { get; } = backgroundJobClient;

    public abstract IImportService ImportService { get; }

    public Guid Id { get; } = Guid.NewGuid();

    public string? JobId { get; set; }

    public Guid? ParentId { get; } = parentId;

    public abstract int SourceId { get; }

    public virtual async Task Enqueue<T>(string queue, Func<T, Task> jobTask, CancellationToken cancellationToken = default)
    {
        var executionJobs = await GetExecutionServiceJobs();
        
        foreach (var executionJob in executionJobs)
        {
            var jobId = BackgroundJobClient.Create<T>(jobExecutor => jobTask(jobExecutor),
                 new EnqueuedState(queue));

            executionJob.Value.JobId = jobId;
        }
    }

    public virtual async Task Execute(string queue, CancellationToken cancellationToken = default)
    {
        JobId = await  _jobExecutor.Execute(queue, this, cancellationToken);
    }

    public virtual async Task Stop(CancellationToken cancellationToken)
    {
        var executingTasks = (await GetExecutionServiceJobs()).Select(e => e.Value.ImportService.Stop(cancellationToken));
        await Task.WhenAll(executingTasks);
    }

    protected virtual Task<IDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs()
    {
        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>() { { Id, this } };
        return Task.FromResult(result);
    }

    Task<IDictionary<Guid, IImportServiceJob>> IImportServiceJob.GetExecutionServiceJobs()
    {
        return GetExecutionServiceJobs();
    }
}