using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.SignalR;
using Serilog;

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
            AddLog(hostContext.Configuration, logging);
        });

        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

        return builder;
    }

    private static void AddLog(IConfiguration configuration, ILoggingBuilder loggingBuilder)
    {
        var logPath = $"{Utils.GetAppPath()}/Logs";
        var logContextFile = "log.property.json";
        var serviceName = "SignalR";
        var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

        loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
        loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(LogHubFilter).GetNameWithoutGenericArity());

        loggerConfiguration.SetSerilog(loggingBuilder);
    }
}
