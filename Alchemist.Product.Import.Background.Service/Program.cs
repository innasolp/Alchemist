using Alchemist.BrowserService.Client;
using Alchemist.Common;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Log.Extensions;
using Alchemist.Product.BeautyAndHealth.ImportItemHandler;
using Alchemist.Product.Category.ImportItemHandler;
using Alchemist.Product.Import.Background;
using Alchemist.Product.Import.Background.Service;
using Alchemist.Product.Import.Background.Settings;
using Alchemist.Settings.RestAPIClient;
using BackgroundTaskQueue;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using DependencyInjection.AssemblyExtensions;
using Hangfire;
using Hangfire.AggregateJobs;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Redis.StackExchange;
using Hangfire.Tags;
using Hangfire.Tags.Redis.StackExchange;
using Import.Factory.Logging;
using Import.Service;
using Import.Settings.Interfaces;
using Message.RabbitMQ.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Configuration.Extensions;
using Serilog.HangfireConsoleContextSink;
using Serilog.Loggers;
using Shop.API.Client;
using Shop.Interfaces;
using ShopImport.Service.Category.Infrastructure;
using ShopImport.Service.Hangfire;
using ShopSettings.Interfaces;
using StackExchange.Redis;
using WebLoader.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

builder.Services.AddKeyedBoundedBackgroundQueue(100, "EventBackgroundTaskQueue");

AddSettingsAdapters(builder);

AddLoaderService(builder);

AddShopAPIService(builder, out var restApiHost);

AddShopSettingsAPIService(builder, out var settingsAPIHost);

var hangfireOptions = builder.Configuration.GetSection("HangfireJobExecuteOptions").Get<AggregateServerSettings>();
builder.Host.AddHangfireServiceManagementInfrastructure(builder.Configuration.GetConnectionString("ServicesStoreRedis"),
    (config, connectionString)=>
    {
        //ClearRedisDataBase(connectionString);
        config.UseRedisStorage(connectionString, new RedisStorageOptions
        {
            // Увеличьте этот таймаут, если задача длится дольше 30 минут
            InvisibilityTimeout = TimeSpan.FromHours(3),
            ExpiryCheckInterval = TimeSpan.FromMinutes(1)
        });

        config.UseTagsWithRedis(new TagsOptions { TagColor = "#1e8700" });
    },
    (options)=> options.UseNpgsql(builder.Configuration.GetConnectionString("ChildJobStoragePostgres")),
    hangfireOptions,
    (config) =>
    {
        if (DashboardRoutes.Routes.FindDispatcher("/console/1234567890a_suffix")?.Item1 == null)
            config.UseConsole();
        
        config.AddAggregateConsoleContextFilter();
    });


AddMessages(builder);

AddShopImporters(builder.Services, builder.Configuration);

AddShopImportItemHandlers(builder.Services, builder.Configuration);

AddLogging(builder.Configuration, builder.Logging, builder.Environment, restApiHost, settingsAPIHost);

builder.Services.AddHostedService<ShopImportWorker>();
builder.Services.AddHostedService<ImportBackgroundTaskQueueHostedService>();
builder.Services.AddHostedService<EventBackgroundTaskQueueHostedService>();

builder.Services.AddAuthentication("https");

var app = builder.Build();

app.UseAuthentication();

//app.UseHsts();

app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseRouting();

app.UseHangfireDashboard("/hangfire");
app.UseImportServiceChildJobOrchestrator(hangfireOptions);

app.MapGet("/", () => "Hello ImportBackgroundService!");

app.UseDefaultFiles();

await app.RunAsync();

static void AddMessages(WebApplicationBuilder builder)
{    
    builder.Services.AddSignalRMessageSender(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageSenderKey);
    builder.Services.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageReceiverKey);
    builder.Services.AddShopImportDataAckReceiver(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.AckEventMessageReceiverKey);
}

static void AddSettingsAdapters(WebApplicationBuilder builder)
{
    builder.Services.AddSettingsDataAdapterToCollection<ProductShopImportSettings, ImportServiceSettings>(ShopSettingType.Product, 
        ShopImportWorkerKeys.ProcessedImportSettings);
    builder.Services.AddSettingsDataAdapterToCollection<CategoryShopImportSettings, ImportServiceSettings>(ShopSettingType.Category,
        ShopImportWorkerKeys.ProcessedImportSettings);
    builder.Services.AddKeyedSettingsJsonAdapter<ProductShopImportSettings>("ImportSettings/shopProducts.json", ShopImportWorkerKeys.InitImportSettings);
    builder.Services.AddKeyedSettingsJsonAdapter<CategoryShopImportSettings>("ImportSettings/shopCategories.json", ShopImportWorkerKeys.InitImportSettings);
}

static void AddLoaderService(WebApplicationBuilder builder)
{
    var browserApiHost = builder.Configuration.GetSection("BrowserServiceHost").Get<string>();
    builder.Services.AddBrowserServiceClientFactory(browserApiHost);

    var webLoaderPath = builder.Configuration.GetSection("WebLoaderPath").Value;
    builder.Services.AddServiceImplementationsFromPath(typeof(IWebLoaderFactory), $"{Utils.GetAppPath()}\\{webLoaderPath}");
}

static void AddShopAPIService(WebApplicationBuilder builder, out string restApiHost)
{
    restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>();
    builder.Services.ConfigureDefaultHttps();
    builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient), out var shopHttpClientBuilder);
}

static void AddShopSettingsAPIService(WebApplicationBuilder builder, out string settingsAPIHost)
{
    settingsAPIHost = builder.Configuration.GetSection("SettingsAPIHost").Get<string>();
    builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out var settingsHttpClientBuilder);
}

static void ClearRedisDataBase(string hangfireConnectionString)
{
    var redis = ConnectionMultiplexer.Connect($"{hangfireConnectionString},allowAdmin=true");

    var endpoints = redis.GetEndPoints();
    foreach (var endpoint in endpoints)
    {
        var server = redis.GetServer(endpoint);
        server.FlushDatabase();
    }
}

static void AddShopImporters(IServiceCollection services, IConfiguration configuration)
{
    services.RegisterServiceImplementationsFromPathDILoad($"{Utils.GetAppPath()}\\{configuration.GetSection("ShopImportServiceStatePath").Value}");
    services.AddMemoryCache();
    services.RegisterServiceImplementationsFromPathDILoad($"{Utils.GetAppPath()}\\{configuration.GetSection("ShopProductImportPath").Value}");

    var categoryServicesPath = $"{Utils.GetAppPath()}\\{configuration.GetSection("ShopCategoryImportPath").Value}";
    var categoryLoadersPath = $"{Utils.GetAppPath()}\\{configuration.GetSection("ShopCategoryLoadersPath").Value}";
    services.AddImportCategoryInfrastructure(categoryServicesPath, categoryLoadersPath);

    services.AddImportServiceLogFactory((logger, name, shopModel, settings) =>
            GetHangfireConsoleLogger(GetShopImportServiceCoreLogger(logger, name, settings)));
}

static Microsoft.Extensions.Logging.ILogger GetShopImportServiceCoreLogger(Microsoft.Extensions.Logging.ILogger logger,  string name, IImportSettings settings)
{
    var shopSettingsType = settings is IShopSettings shopSettings && shopSettings != null ? shopSettings.Type : ShopSettingType.Service;

    return new SerilogPropertyLogger(logger, new Dictionary<string, object>{
            { "ShopImportService", name },
            { "ShopSettingsType", shopSettingsType.ToString() } });
}

static Microsoft.Extensions.Logging.ILogger GetHangfireConsoleLogger(Microsoft.Extensions.Logging.ILogger logger)
{
    return new SerilogAssemblyResourcePropertyLogger(logger,
            new Dictionary<string, System.Reflection.Assembly>()
            {
                { "Import.Service.LogMessages", typeof(ImportService).Assembly }, 
                { "Import.Service.AggregateLogMessages", typeof(ImportService).Assembly }, 
            },
            new Dictionary<string, Dictionary<string, object>>
            {
                { "Import.Service.LogMessages", new Dictionary<string, object>{ { "Hangfire", true } } },
                { "Import.Service.AggregateLogMessages", new Dictionary<string, object>{ { "HangfireAggregate", true } } }
            });
}

static void AddShopImportItemHandlers(IServiceCollection services, IConfiguration configuration)
{
    var rabbitMQOptions = configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
    services.AddRabbitMQMessageSender("importqueue", rabbitMQOptions);
    
    services.AddImportProductMessageSender((s, key)=>s.AddSignalRMessageSender(configuration, "SignalRImportUrl", key));
    services.AddImportCategoryMessageSender((s, key)=>s.AddSignalRMessageSender(configuration, "SignalRImportUrl", key));

    services.AddKeyedUnboundedBackgroundQueue("ImportBackgroundTaskQueue");
    services.AddProductQueueItemHandlerFactory("importqueue", configuration.GetSection("RabbitMQProductEvent").Get<string>());
    services.AddCategoryQueueItemHandler("importqueue", configuration.GetSection("RabbitMQCategoryEvent").Get<string>());
}

static void AddShopImportLogging(string logPath, IWebHostEnvironment environment, LoggerConfiguration loggerConfiguration)
{
    loggerConfiguration.AddContextPropertyConfig(logContextFile: "log.contextproperty.json",
         logPath: $"{logPath}/Import/Products",
         contextPropertyName: "ShopImportService",
        sourceContext: "Import",
        propertyExpressions: [new PropertyExpression(SerilogFunc.Contains, [new ContextProperty("ShopSettingsType"), ShopSettingType.Product.ToString()])]);

    loggerConfiguration.AddContextPropertyConfig(logContextFile: "log.contextproperty.json",
         logPath: $"{logPath}/Import/Categories",
         contextPropertyName: "ShopImportService",
        sourceContext: "Import",
        propertyExpressions: [new PropertyExpression(SerilogFunc.Contains, [new ContextProperty("ShopSettingsType"), ShopSettingType.Category.ToString()])]);
}

static void AddHangfireLogging(LoggerConfiguration loggerConfiguration)
{
    loggerConfiguration.AddContextPropertiesConfig("log.hangfire.json", new Dictionary<string, string[]>()
    {
        { "$[ContextProperties]", ["ShopImportService"] },
        { "$[JobParameters]", ["displayName"] }
    },
    "ShopImportService");
}

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder, IWebHostEnvironment environment,  string? restApiHost, string? settingsAPIHost)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    AddShopImportLogging(logPath, environment, loggerConfiguration);
    
    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(ShopImportWorker).Name);
    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(EventBackgroundTaskQueueHostedService).Name);
    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(ImportBackgroundTaskQueueHostedService).Name);

    AddHangfireLogging(loggerConfiguration);

    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ImportBackgroundServiceProgram
{ }