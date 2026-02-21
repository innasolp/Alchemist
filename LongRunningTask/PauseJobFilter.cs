using Hangfire.Common;
using Hangfire.Server;

namespace LongRunningTask;

public class PauseJobFilter : JobFilterAttribute, IServerFilter
{
    public void OnPerformed(PerformedContext context)
    {
        //var pausedJobKey = $"paused_{context.BackgroundJob.Id}";
        //if(context.Items.TryAdd(pausedJobKey, true))
        //        context.Items[pausedJobKey] = true;            

        //JobControl.PauseJob(context.BackgroundJob.Id);
        //todo
    }

    public void OnPerforming(PerformingContext context)
    {
        //var pausedJobKey = $"paused_{context.BackgroundJob.Id}";
        //if (!context.Items.TryGetValue(pausedJobKey, out var jobPausedData) || jobPausedData is not bool)
        //{
        //    context.Canceled = true;           
        //}
        //
        if (JobControl.IsPaused(context))
            JobControl.ResumeJob(context);
        else
            JobControl.PauseJob(context);
    }
}