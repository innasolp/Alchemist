using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal abstract class ImportServiceJob(Guid? parentId = null) : IImportServiceJob
{
    public abstract IImportService ImportService { get; }

    public Guid Id { get; } = Guid.NewGuid();

    public string? JobId { get; set; }

    public Guid? ParentId { get; } = parentId;

    public abstract int SourceId { get; }    

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