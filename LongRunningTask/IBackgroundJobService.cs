using Hangfire.Server;

namespace LongRunningTask;

public interface IBackgroundJobService
{
    [PauseJobFilter]
    Task Pause(IJobService jobService, PerformContext? filterContext);

    Task Execute(IJobService jobService, PerformContext? filterContext);

    [PauseJobFilter]
    Task Resume(IJobService jobService, PerformContext? filterContext);

    Task Stop(IJobService jobService, PerformContext? filterContext);
}