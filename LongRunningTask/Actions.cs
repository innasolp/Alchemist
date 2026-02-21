using Hangfire;
using Hangfire.Dashboard.Management.v2.Metadata;
using Hangfire.Server;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTask;

[ManagementPage]
public class Actions(IBackgroundJobClient backgroundJobClient)
{
    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    [DisplayName("Resume")]
    public void ResumeJob(PerformContext context)
    {
        _backgroundJobClient.ChangeState(context.BackgroundJob.Id, new EnqueuedState());
    }

    [DisplayName("Pause")]
    public void PauseJob(PerformContext context)
    {
        _backgroundJobClient.ChangeState(context.BackgroundJob.Id, new PausedState());
    }
}
