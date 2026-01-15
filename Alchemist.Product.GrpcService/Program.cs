using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.GrpcService.Services;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Grpc.Server.Interceptors;
using Grpc.Server.RequestInterceptor;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Loggers;
using Alchemist.Product.Module;

internal class Program
{
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
            {
                var ex = e.ExceptionObject as Exception;
                Serilog.Log.Fatal(ex, ex?.Message ?? $"Fatal error. {AppDomain.CurrentDomain.FriendlyName} will be terminated.");
            }
        }

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
        builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

        builder.Services.AddPerfomanceCounter<ServerRequestSenderInterceptor<AlchemyService>>((logger) => new SerilogUrlLogger<PerfomanceCounter<ServerRequestSenderInterceptor<AlchemyService>>>(logger));

        builder.Services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")));

        builder.Host.AddProductInfrastructure();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterGenericHandlers = true;
            cfg.RegisterServicesFromAssemblies(assemblies);
        });


        builder.Services.AddSingleton<ServerLoggingInterceptor<AlchemyService>>();
        builder.Services.AddSingleton<ServerRequestSenderInterceptor<AlchemyService>>();
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ServerRequestSenderInterceptor<AlchemyService>>();
            options.Interceptors.Add<ServerLoggingInterceptor<AlchemyService>>();
        });

        AddLogging(builder.Configuration, builder.Logging);

        builder.Services.AddAuthentication("https");

        var app = builder.Build();

        app.UseAuthentication();

        // Configure the HTTP request pipeline.

        if (builder.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseHsts();

        app.UseHttpsRedirection();

        app.UseRouting();

        (app as IHost).UsePerfomanceCounters();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<AlchemyService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();

        static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
        {
            var logPath = $"{Utils.GetAppPath()}/Logs";
            var logContextFile = "log.property.json";
            var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

            loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(AlchemyService).Name);
            loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{typeof(AlchemyService).Name}", typeof(ServerRequestSenderInterceptor<>).GetNameWithoutGenericArity());
            loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{typeof(AlchemyService).Name}", typeof(ServerLoggingInterceptor<>).GetNameWithoutGenericArity());

            loggerConfiguration.AddPerfomanceCounter(logContextFile, logPath, url: "https://localhost:8071", EventIds.Perfomance.Id, typeof(AlchemyService).Name);

            loggerConfiguration.SetSerilog(loggingBuilder);
        }
    }
}

public partial class GrpcServiceProgramm
{
}
