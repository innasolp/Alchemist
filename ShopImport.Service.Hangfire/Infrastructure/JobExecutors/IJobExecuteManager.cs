namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal interface IJobExecuteManager
{
    Task Enqueue<T>(IImportServiceJob importServiceJob,
        Func<T, IImportServiceJob, Task> execute,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default);

    Task Execute(IImportServiceJob importServiceJob,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default);

    Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);
}