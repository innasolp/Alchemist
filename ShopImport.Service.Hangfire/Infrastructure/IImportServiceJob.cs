using Import.Interfaces;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IImportServiceJob
{
    IImportService ImportService { get; }

    public int SourceId { get; }

    Guid Id { get; }

    string? JobId { get; set; }

    Guid? ParentId { get; }

    Task<IDictionary<Guid, IImportServiceJob>> GetExecutionServiceJobs();

    Task Enqueue<T>(string queue, Func<T, Task> jobTask, CancellationToken cancellationToken = default);

    Task Execute(string queue, CancellationToken cancellationToken = default);

    Task Stop(CancellationToken cancellationToken = default);
}