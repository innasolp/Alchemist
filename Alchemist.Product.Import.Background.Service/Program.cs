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
using Import.Factory.Logging;
using Message.RabbitMQ.DependencyInjection;
using Serilog;
using Serilog.Configuration.Extensions;
using Serilog.Loggers;
using Shop.API.Client;
using Shop.Interfaces;
using ShopImport.Service.Category.Infrastructure;
using ShopImport.Service.Hangfire;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Infrastructure.Module;
using ShopSettings.Interfaces;
using WebLoader.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

builder.Services.AddKeyedBoundedBackgroundQueue(100, "EventBackgroundTaskQueue");

AddSettingsAdapters(builder);

AddLoaderService(builder);

AddShopAPIService(builder, out var restApiHost);

AddShopSettingsAPIService(builder, out var settingsAPIHost);

//builder.Host.AddImportServicesInfrastructure();
var hangfireOptions = builder.Configuration.GetSection("HangfireJobExecuteOptions").Get<JobExecuteOptions>();
builder.Host.AddImportServicesInfrastructure(builder.Configuration.GetConnectionString("ServicesStoreRedis"), hangfireOptions);

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

app.MapGet("/", () => "Hello ImportBackgroundService!");

app.UseDefaultFiles();

await app.RunAsync();

static void AddMessages(WebApplicationBuilder builder)
{    
    builder.Services.AddSignalRMessageSender(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageSenderKey);
    builder.Services.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageReceiverKey);
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

static void AddShopImporters(IServiceCollection services, IConfiguration configuration)
{
    services.RegisterServiceImplementationsFromPathDILoad($"{Utils.GetAppPath()}\\{configuration.GetSection("ShopImportServiceStatePath").Value}");
    services.AddMemoryCache();
    services.RegisterServiceImplementationsFromPathDILoad($"{Utils.GetAppPath()}\\{configuration.GetSection("ShopProductImportPath").Value}");

    var categoryServicesPath = $"{Utils.GetAppPath()}\\{configuration.GetSection("ShopCategoryImportPath").Value}";
    var categoryLoadersPath = $"{Utils.GetAppPath()}\\{configuration.GetSection("ShopCategoryLoadersPath").Value}";
    services.AddImportCategoryInfrastructure(categoryServicesPath, categoryLoadersPath);

    services.AddImportServiceLogFactory((logger, name, shopModel, settings) => new SerilogPropertyLogger(logger, new Dictionary<string, object>{
    { "ShopImportService", name },
    { "ShopSettingsType", (settings as IShopSettings).Type.ToString() } }));
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

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder, IWebHostEnvironment environment,  string? restApiHost, string? settingsAPIHost)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    AddShopImportLogging(logPath, environment, loggerConfiguration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(ShopImportWorker).Name);
    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(EventBackgroundTaskQueueHostedService).Name);
    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(ImportBackgroundTaskQueueHostedService).Name);

    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ImportBackgroundServiceProgram
{ }