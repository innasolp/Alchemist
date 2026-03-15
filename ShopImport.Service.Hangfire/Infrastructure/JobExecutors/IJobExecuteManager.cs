namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal interface IJobExecuteManager
{
    Task Enqueue(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);

    Task Execute(IImportServiceJob importServiceJob, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);

    Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);
}
