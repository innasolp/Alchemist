using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.GrpcService.Services;
using Alchemist.Product.Module;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Mapster;
using Mediator.Module.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using GrpcExtensions.Aspnet.Interceptors;


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

builder.WebHost.UseKestrel();

builder.Services.AddMapster();
TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);


builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

builder.Services.AddAlchemyPostgresContextFactory(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext2")));

builder.Host.AddMediatorInfrastructure<ProductModule>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GrpcHttpLoggingInterceptor>();
    options.Interceptors.Add<GrpcExceptionHandlerInterceptor>();
}).AddJsonTranscoding();

// Register OpenAPI/Swagger support for transcoded gRPC endpoints
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "gRPC Product service", Version = "v1" });

    var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory ?? ".", xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
        c.IncludeGrpcXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});
builder.Services.ConfigureSwagger((options) =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
});

var logger = AddLogging(builder.Configuration, builder.Logging);
builder.Host.UseSerilog(logger);

builder.Services.AddAuthentication("https");

var app = builder.Build();

app.UseAuthentication();

// Configure the HTTP request pipeline.

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseDeveloperExceptionPage();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "gRPC Product service v1");
    });
}

app.UseHsts();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

await app.UseAlchemyPostgresqlMigrationAsync();

app.UseRouting();

// Configure the HTTP request pipeline.
app.MapGrpcService<AlchemyService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

static Serilog.ILogger AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(AlchemyService).Name);
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{typeof(AlchemyService).Name}/Grpc", nameof(GrpcHttpLoggingInterceptor));
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{typeof(AlchemyService).Name}/Grpc", nameof(GrpcExceptionHandlerInterceptor));

    return loggerConfiguration.SetSerilog(loggingBuilder);
}


public partial class GrpcServiceProgramm
{
}
