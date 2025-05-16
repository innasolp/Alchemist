using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.SignalR;
using Serilog.Configuration.Extensions;

public partial class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);       

        builder.ConfigureLogging((hostContext, logging) =>
        {
            var appSerilogBuilder = SetLog(hostContext.Configuration, hostContext.HostingEnvironment);
            appSerilogBuilder.SetSerilog(logging);
        });

        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

        return builder;
    }

    private static SerilogConfigurationBuilder SetLog(IConfiguration configuration, IHostEnvironment env)
    {
        var logPath = $"{Utils.GetAppPath()}/Logs";
        var logContextPath = $"{env.ContentRootPath}/log.property.json";
        var serviceName = "SignalR";
        var appSerilogBuilder = new SerilogConfigurationBuilder(configuration);
        appSerilogBuilder.AddServiceBaseConfigs(logContextPath, logPath, serviceName);
        appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(LogHubFilter).GetNameWithoutGenericArity());
        return appSerilogBuilder;
    }
}
