using Hangfire;
using Hangfire.States;

namespace ShopImport.Service.Hangfire.Infrastructure;

internal class SingleBackgroundJobExecutor(IBackgroundJobClient backgroundJobClient) : IJobExecutor
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    public Task<string> Execute(string queue, IImportServiceJob importServiceJob, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_backgroundJobClient.Create<IHagfireServiceJobManager>(
                     serviceJobManager => serviceJobManager.Execute(importServiceJob.Id,
                                                                    importServiceJob.ImportService.Name,
                                                                    cancellationToken,
                                                                    null),
                     new EnqueuedState(queue)));
    }
}