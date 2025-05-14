using Alchemist.Product.GrpcService.Services;
using Microsoft.EntityFrameworkCore;
using Alchemist.Common;
using Grpc.Server.Interceptors;
using Alchemist.Product.Data.Repository;
using Alchemist.Log.Serilog;
using Grpc.Server.RequestInterceptor;
using Http.RequestHandling.PerfomanceCounter;
using Serilog.Loggers;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Data;

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

builder.Services.AddPerfomanceCounter<ServerRequestSenderInterceptor<AlchemyService>>((logger) => new SerilogUrlLogger<PerfomanceCounter<ServerRequestSenderInterceptor<AlchemyService>>>(logger));

builder.Services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")?.SetEnvironmentLocalHostIfNeed()));
builder.Services.AddScoped<IAlchemyRepository,AlchemyRepository>();

builder.Services.AddSingleton<ServerLoggingInterceptor<AlchemyService>>();
builder.Services.AddSingleton<ServerRequestSenderInterceptor<AlchemyService>>();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServerRequestSenderInterceptor<AlchemyService>>();
    options.Interceptors.Add<ServerLoggingInterceptor<AlchemyService>>();    
});

var logPath = $"{Utils.GetAppPath()}/Logs";
var appSerilogBuilder = new AppSerilogBuilder(builder.Configuration, builder.Environment);
appSerilogBuilder.AddServiceBaseConfigs(typeof(AlchemyService).Name);
appSerilogBuilder.AddSourceContextContainsLogConfig($"{logPath}/{typeof(AlchemyService).Name}", typeof(ServerRequestSenderInterceptor<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextContainsLogConfig($"{logPath}/{typeof(AlchemyService).Name}", typeof(ServerLoggingInterceptor<>).GetNameWithoutGenericArity());

appSerilogBuilder.AddPerfomanceCounter(url: "https://localhost:8071", EventIds.Perfomance.Id, logPath, typeof(AlchemyService).Name);

appSerilogBuilder.SetSerilog(builder.Logging);

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

public partial class Program
{
}
