using Alchemist.Common;
using Alchemist.Log.Serilog;
using Alchemist.Product.SignalR;

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

    private static AppSerilogBuilder SetLog(IConfiguration configuration, IHostEnvironment env)
    {
        var logPath = $"{Utils.GetAppPath()}/Logs";
        var serviceName = "SignalR";
        var appSerilogBuilder = new AppSerilogBuilder(configuration, env);
        appSerilogBuilder.AddServiceBaseConfigs(serviceName);
        appSerilogBuilder.AddSourceContextContainsLogConfig($"{logPath}/{serviceName}", typeof(LogHubFilter).GetNameWithoutGenericArity());
        return appSerilogBuilder;
    }
}
