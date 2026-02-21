using Hangfire.Dashboard;
using Hangfire.AspNetCore;
using System.Reflection;

namespace LongRunningTask;

public static class PausedJobsPageConfig
{
    public static void Register()
    {
        DashboardRoutes.Routes.Add("/jobs/paused", new PauseResumeJobDispatcher());

        NavigationMenu.Items.Add((page) =>
            new MenuItem("Paused", page.Url.To("/jobs/paused"))
            {
                Active = page.Url.ToString()?.StartsWith("/jobs/paused") == true,
                Metric = new DashboardMetric("jobsPausedMetric", page=>GetMetric(page))
            }
        );
    }

    private static Metric GetMetric(RazorPage page)
    {
        using var connection = page.Storage.GetConnection();
        var monitoring = page.Context.Storage.GetMonitoringApi();
        return new Metric(monitoring.GetStatistics().Enqueued.ToString());
    }


    public static void AddResume()
    {
        DashboardRoutes.Routes.Add("/js/paused-actions.js", new PauseResumeJobDispatcher());
    }
}