using Hangfire.Dashboard;

namespace LongRunningTask;

public class JobControlDispatcher : IDashboardDispatcher
{
    public async Task Dispatch(DashboardContext context)
    {
        if (!"POST".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 405;
            return;
        }

        var jobId = context.Request.GetQuery("jobId");
        var action = context.Request.GetQuery("action");

        if (action == "pause")
            JobControl.PauseJob(jobId); 
        else if (action == "resume")
            JobControl.ResumeJob(jobId);

        context.Response.StatusCode = 200;
    }
}
