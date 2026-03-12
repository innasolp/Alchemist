namespace ShopImport.Service.Hangfire.Infrastructure;

internal interface IJobExecutor
{
    Task<string> Execute(string queue, IImportServiceJob importServiceJob, CancellationToken cancellationToken = default);
}