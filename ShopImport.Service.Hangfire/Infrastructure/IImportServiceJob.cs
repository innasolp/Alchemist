using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IImportServiceJob
{
    IImportService ImportService { get; }

    public int SourceId { get; }

    Guid Guid { get; }

    string? JobId { get; set; }

    Guid? ParentId { get; }

    Task<IDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs();
}