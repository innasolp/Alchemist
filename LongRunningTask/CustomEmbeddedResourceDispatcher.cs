using Hangfire.Dashboard;
using System.Reflection;

namespace LongRunningTask;

internal class CustomEmbeddedResourceDispatcher(Assembly assembly, string resourceName) : IDashboardDispatcher
{
    private readonly Assembly _assembly = assembly;
    private readonly string _resourceName = resourceName;

    public async Task Dispatch(DashboardContext context)
    {
        // Устанавливаем тип контента (для JS это application/javascript)
        context.Response.ContentType = "application/javascript";

        using (var stream = _assembly.GetManifestResourceStream(_resourceName))
        {
            if (stream == null)
            {
                context.Response.StatusCode = 404;
                return;
            }
            await stream.CopyToAsync(context.Response.Body);
        }
    }
}