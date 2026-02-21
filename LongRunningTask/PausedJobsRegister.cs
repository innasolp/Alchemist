using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using System.Reflection;
namespace LongRunningTask;

public static class PausedJobsRegister
{
    public static void UseHangfireSuspendDashboard(IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            IgnoreAntiforgeryToken = true
        });

        //// Регистрация кастомного пути для кнопки
        //DashboardRoutes.Routes.Add("/pause", new PauseResumeJobDispatcher());

        DashboardRoutes.Routes.Add("/js/paused-actions.js", 
            new CustomEmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "LongRunningTask.js.paused-actions.js"));

        DashboardRoutes.Routes.Add("/job-control", new JobControlDispatcher());
    }

    public static void UseHangfireSuspendPage(this IApplicationBuilder app)
    {
        DashboardRoutes.Routes.AddRazorPage("/suspendJobs",
            page => new CustomJobsPage());

        NavigationMenu.Items.Add(
            menu => new MenuItem("suspend Jobs", menu.Url.To("/suspendJobs")));

        DashboardRoutes.Routes.AddCommand("/suspendJobs/(?<JobId>.+)/resume",
            context =>
            {
                JobPausedStorage.MarkAsResume(context.UriMatch.Groups["JobId"].Value);
                return true;
            });

        DashboardRoutes.Routes.AddCommand("/suspendJobs/(?<JobName>.+)/pause",
            context =>
            {
                JobPausedStorage.MarkAsPause(context.UriMatch.Groups["JobName"].Value);
                return true;
            });

        app.UseHangfireDashboard("/hangfire");
    }

}