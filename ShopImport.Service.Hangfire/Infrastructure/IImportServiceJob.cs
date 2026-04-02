using Hangfire.AggregateJobs;
using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IImportServiceJob
{
    IImportService ImportService { get; }

    public int SourceId { get; }

    Guid Id { get; }

    string? JobId { get; set; }

    Guid? ParentId { get; }

    JobExecuteOptions? JobExecuteOptions { get; }

    Task<IReadOnlyDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs();

    bool IsAggregate { get; }
}