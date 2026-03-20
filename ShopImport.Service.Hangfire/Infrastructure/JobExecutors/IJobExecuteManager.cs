using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal interface IJobExecuteManager
{
    Task Enqueue<T>(IImportServiceJob importServiceJob,
        Expression<Func<T, IImportServiceJob, Task>> execute,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default);

    Task Execute<T>(IImportServiceJob importServiceJob,
        JobExecuteOptions jobExecuteOptions,
        CancellationToken cancellationToken = default);

    Task StopWithFailedState(IImportServiceJob importServiceJob, 
        Exception exception, 
        JobExecuteOptions? jobExecuteOptions = null, 
        CancellationToken cancellationToken = default);
}