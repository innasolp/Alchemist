using Alchemist.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.WebApp.Api.Common;
using BackgroundTaskQueue;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Db.Infrastructure.EF.Outbox;
using Db.Infrastructure.Messages;
using Http.ErrorHandling;
using Log.Interceptors;
using Message.SignalR.HubMessage.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Loggers;
using Shop.API;
using Shop.Data.Infrastructure.EF;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

// Add services to the container.

var supportedEventTypes = builder.Configuration.GetSection("SupportedEvents").Get<List<SupportedEventType>>();

builder.Services.AddAlchemyPostgresContextFactory((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext2"));
    options.AddOutbox(sp, supportedEventTypes);
});
builder.Services.AddOutboxProcessor<AlchemyContext>();
builder.Services.AddUnboundedBackgroundQueue();

builder.Host.AddShopInfrastructure((builder)=>builder.AddCallbackBackgroundMessageHandlers());

var signalRUrl = builder.Configuration.GetSection("SignalRUrl").Get<string>();
builder.Services.AddSignalRMessageHubAcknowledgefulSender(signalRUrl);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerApi();

builder.Services.AddAuthentication("https");

builder.Services.AddBaseControllerInterceptors();

var logger = AddLogging(builder.Configuration, builder.Logging);

builder.Host.UseSerilog(logger);

InterceptLogs(builder.Services);

builder.Services.AddHostedService<ShopBackgroundTaskQueuedHostedService>();

var app = builder.Build();

app.UseAuthentication();

app.UseBaseInterceptors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.UseApiSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHsts();
app.MapControllers();

app.UseSerilogRequestLogging();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    
//if (!app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
//{
    await app.UseAlchemyPostgresqlMigrationWithRedisLockIfAvailableAsync("RedisStore");
//}

await app.UseOutbox<AlchemyContext>();

app.Run();

static void InterceptLogs(IServiceCollection services)
{
    services.InterceptLoggerFactory((logger) => new SerilogForceDestructuringLogger(logger));
}

static Serilog.ILogger AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.Shop.RestAPI";

    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware");
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", nameof(GlobalExceptionHandler));
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/{nameof(ShopBackgroundTaskQueuedHostedService)}", nameof(ShopBackgroundTaskQueuedHostedService));

    return loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ShopAPIProgram { }
