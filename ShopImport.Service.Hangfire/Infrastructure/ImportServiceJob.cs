using Hangfire.AggregateJobs;
using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal abstract class ImportServiceJob(Guid id, Guid? parentId = null, JobExecuteOptions? jobExecuteOptions = null) : IImportServiceJob
{
    public abstract IImportService ImportService { get; }

    public Guid Id { get; } = id;

    public string? JobId { get; set; }

    public Guid? ParentId { get; } = parentId;

    public abstract int SourceId { get; }

    public JobExecuteOptions? JobExecuteOptions { get; } = jobExecuteOptions;

    protected abstract bool IsAggregate { get; }

    bool IImportServiceJob.IsAggregate => IsAggregate;

    protected virtual Task<IReadOnlyDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs()
    {
        IDictionary<Guid, IImportServiceJob> result = new Dictionary<Guid, IImportServiceJob>() { { Id, this } };
        return Task.FromResult((IReadOnlyDictionary<Guid, IImportServiceJob>)result);
    }

    Task<IReadOnlyDictionary<Guid, IImportServiceJob>> IImportServiceJob.GetExecutionServiceJobs()
    {
        return GetExecutionServiceJobs();
    }
}