using Alchemist.Common;
using Alchemist.Log.Serilog;
using Http.ErrorHandling;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.EntityFrameworkCore;
using Message.SignalR.DependencyInjection;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.RestAPI;
using Alchemist.Product.Data;

public partial class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

        builder.ConfigureServices((context, services) =>
            {
                services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(context.Configuration.GetConnectionString("DbContext")?
                     .SetEnvironmentLocalHostIfNeed()));

                var signalRUrl = context.Configuration.GetSection("SignalRUrl").Get<string>()?.SetEnvironmentLocalHostIfNeed();
                services.AddSignalRMessageSender(signalRUrl);
            });        

        builder.ConfigureLogging((hostContext, logging) =>
        {
            var appSerilogBuilder = SetLog(hostContext.Configuration, hostContext.HostingEnvironment);
            appSerilogBuilder.SetSerilog(logging);
        });
        
       
        return builder;
    }

    private static AppSerilogBuilder SetLog(IConfiguration configuration, IHostEnvironment env)
    {
        var logPath = $"{Utils.GetAppPath()}/Logs";
        var appSerilogBuilder = new AppSerilogBuilder(configuration, env);
        var serviceName = "Alchemist.Shop.RestAPI";
        appSerilogBuilder.AddServiceBaseConfigs(serviceName);
        appSerilogBuilder.AddPerfomanceCounter(url: "https://localhost:8051", EventIds.Perfomance.Id, logPath, serviceName);
        appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
        appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());
        return appSerilogBuilder;
    }
}
