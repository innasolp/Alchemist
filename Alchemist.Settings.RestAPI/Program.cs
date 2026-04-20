using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Settings.RestAPI;
using BackgroundTaskQueue;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Http.ErrorHandling;
using Log.Interceptors;
using Mediator.Module.EF;
using Message.SignalR.HubMessage.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Loggers;
using ShopSettings.Module;
using Alchemist.WebApp.Api.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

// Add services to the container.

builder.Services.AddAlchemyPostgresContextFactory((sp,options) =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext2")));

builder.Services.AddUnboundedBackgroundQueue();


builder.Host.AddMediatorInfrastructure(new ShopSettingsModule());

var signalRUrl = builder.Configuration.GetSection("SignalRUrl").Get<string>();
builder.Services.AddSignalRHubMessageSender(signalRUrl);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerApi();

builder.Services.AddAuthentication("https");

builder.Services.AddBaseControllerInterceptors();

var logger = AddLogging(builder.Configuration, builder.Logging);

builder.Host.UseSerilog(logger);
InterceptLogs(builder.Services);

builder.Services.AddHostedService<ShopSettingsBackgroundTaskQueuedHostedService>();

var app = builder.Build();

app.UseBaseInterceptors();

app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.UseApiSwagger();

app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

await app.UseAlchemyPostgresqlMigrationWithRedisLockIfAvailableAsync("RedisStore");

app.Run();

static void InterceptLogs(IServiceCollection services)
{
    services.InterceptLoggerFactory((logger) => new SerilogForceDestructuringLogger(logger));
}

static Serilog.ILogger AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.Settings.RestAPI";

    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware");
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", nameof(GlobalExceptionHandler));
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/{nameof(ShopSettingsBackgroundTaskQueuedHostedService)}", nameof(ShopSettingsBackgroundTaskQueuedHostedService));

    return loggerConfiguration.SetSerilog(loggingBuilder);
}

public class SettingsAPIProgram { }