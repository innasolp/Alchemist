using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Log.Serilog;

public static class SerilogExtensions
{
    public static AppSerilogBuilder AddAppSerilogLogging(this IHostApplicationBuilder builder)
    {
        var appLogging = new AppSerilogBuilder(builder);
        builder.Services.AddSingleton(appLogging);
        return appLogging;
    }
}
