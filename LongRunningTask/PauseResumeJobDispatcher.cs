using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace LongRunningTask;

public class PauseResumeJobDispatcher : IDashboardDispatcher
{
    public async Task Dispatch([NotNull] DashboardContext context)
    {
        //var monitoringApi = context.Storage.GetMonitoringApi();

        //todo get paused jobs from monitoringApi and rendering

        await context.Response.WriteAsync("<h1>Paused jobs list</h1>");

        var jobId = context.Request.GetQuery("jobId");

        var connection = context.Storage.GetConnection();

        var jobPausedKey = $"paused-job:{jobId}";

        var hashSet = connection.GetAllEntriesFromHash(jobPausedKey);
        if (hashSet.TryGetValue(JobControl.PausedHashKey, out var paused) && paused == "paused")
            JobControl.ResumeJob(jobId);
        else JobControl.PauseJob(jobId);

        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("Job Paused");
    }
}