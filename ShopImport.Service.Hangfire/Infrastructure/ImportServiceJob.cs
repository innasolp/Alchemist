using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal abstract class ImportServiceJob(Guid? parentId = null, ServiceExecuteOptions? serviceExecuteOptions = null) : IImportServiceJob
{
    public abstract IImportService ImportService { get; }

    public Guid Id { get; } = Guid.NewGuid();

    public string? JobId { get; set; }

    public Guid? ParentId { get; } = parentId;

    public abstract int SourceId { get; }

    public ServiceExecuteOptions? ServiceExecuteOptions { get; } = serviceExecuteOptions;

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