namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal interface IJobExecutor
{
    Task Enqueue(IImportServiceJob importServiceJob, bool isAggregate = false, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);

    Task Execute(IImportServiceJob importServiceJob, bool isAggregate = false, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);

    Task StopWithFailedState(IImportServiceJob importServiceJob, Exception exception, JobExecuteOptions? jobExecuteOptions = null, CancellationToken cancellationToken = default);
}