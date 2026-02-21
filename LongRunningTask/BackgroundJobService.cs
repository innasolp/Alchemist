using Hangfire.Server;

namespace LongRunningTask;

public class BackgroundJobService  : IBackgroundJobService
{
    public async Task Pause(IJobService jobService, PerformContext? filterContext)
    {
        await jobService.Pause(filterContext?.CancellationToken.ShutdownToken ?? default);
    }

    public async Task Execute(IJobService jobService, PerformContext? filterContext)
    {
        await jobService.Execute(filterContext?.CancellationToken.ShutdownToken ?? default);
    }

    public async Task Resume(IJobService jobService, PerformContext? filterContext)
    {
        await jobService.Resume(filterContext?.CancellationToken.ShutdownToken ?? default);        
    }

    public Task Stop(IJobService jobService, PerformContext? filterContext)
    {
        return jobService.Stop(filterContext?.CancellationToken.ShutdownToken ?? default);
    }
}