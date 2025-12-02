using Alchemist.BrowserService.Client;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Log.Extensions;
using Alchemist.Product.Import.Background;
using Alchemist.Product.Import.Background.Settings;
using Alchemist.Product.Interfaces;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using DependencyInjection.AssemblyExtensions;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Message.RabbitMQ.DependencyInjection;
using Serilog.Configuration.Extensions;
using Serilog.Loggers;
using WebLoader.Interfaces;
using Alchemist.Product.ImportItemHandler;
using CustomJsonConfigurationProvider;
using CustomConfigurationProvider;
using Serilog;
using Import.Factory.Interfaces;
using Import.Factory.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

AddSettingsAdapters(builder);

AddLoaderService(builder);

AddShopAPIService(builder, out var restApiHost);

AddShopSettingsAPIService(builder, out var settingsAPIHost);

builder.Services.AddPerfomanceCounter<RequestDelegatingHandler>((logger) => new SerilogUrlLogger<PerfomanceCounter<RequestDelegatingHandler>>(logger));

AddMessages(builder);

AddShopImporters(builder);

AddLogging(builder.Configuration, builder.Logging, builder.Environment, restApiHost, settingsAPIHost);

builder.Services.AddHostedService<ShopImportWorker>();

builder.Services.AddAuthentication("https");

var app = builder.Build();

app.UseAuthentication();

//app.UseHsts();

app.UseHttpsRedirection();

(app as IHost).UsePerfomanceCounters();

//app.UseAuthorization();

app.UseRouting();

app.MapGet("/", () => "Hello ImportBackgroundService!");

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
    builder.Services.AddKeyedSettingsJsonAdapter<ProductShopImportSettings>("shopProducts.json", ShopImportWorkerKeys.InitImportSettings);
    builder.Services.AddKeyedSettingsJsonAdapter<CategoryShopImportSettings>("shopCategories.json", ShopImportWorkerKeys.InitImportSettings);
}

static void AddLoaderService(WebApplicationBuilder builder)
{
    builder.Services.AddKeyedSingleton(nameof(BrowserServiceClientFactory),
           builder.Configuration.GetSection("BrowserServiceHost").Get<string>());
    builder.Services.AddSingleton<ILoaderServiceFactory, BrowserServiceClientFactory>();
    builder.Services.AddServiceImplementationsFromPath(typeof(IWebLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("WebLoaderPath").Value}");
}

static void AddShopAPIService(WebApplicationBuilder builder, out string restApiHost)
{
    restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>();
    builder.Services.ConfigureDefaultHttps();
    builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient), out var shopHttpClientBuilder);
    builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(shopHttpClientBuilder);
}

static void AddShopSettingsAPIService(WebApplicationBuilder builder, out string settingsAPIHost)
{
    settingsAPIHost = builder.Configuration.GetSection("SettingsAPIHost").Get<string>();
    builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out var settingsHttpClientBuilder);
    builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(settingsHttpClientBuilder);
}

static void AddShopImporters(WebApplicationBuilder builder)
{
    builder.Services.AddServiceImplementationsFromPath(typeof(IImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopProductImportPath").Value}");
    builder.Services.AddServiceImplementationsFromPath(typeof(IImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopCategoryImportPath").Value}");

    builder.Services.AddImportServiceLogFactory((logger, name, shopModel, settings) => new SerilogPropertyLogger(logger, new Dictionary<string, object>{
    { "ShopImportService", name },
    { "ShopSettingsType", (settings as IShopSettings).Type.ToString() } }));

    builder.Services.AddPerfomanceCounter((logger) => new SerilogUrlLogger<IPerfomanceCounter>(logger));

    AddShopImportItemHandlers(builder.Services, builder.Configuration);
}

static void AddShopImportItemHandlers(IServiceCollection services, IConfiguration configuration)
{
    var rabbitMQOptions = configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
    services.AddRabbitMQMessageSender("importqueue", rabbitMQOptions);
    
    services.AddImportProductMessageSender((s, key)=>s.AddSignalRMessageSender(configuration, "SignalRImportUrl", key));
    services.AddImportCategoryMessageSender((s, key)=>s.AddSignalRMessageSender(configuration, "SignalRImportUrl", key));

    services.AddProductItemHandler("importqueue", configuration.GetSection("RabbitMQProductEvent").Get<string>());
    services.AddCategoryItemHandler("importqueue", configuration.GetSection("RabbitMQCategoryEvent").Get<string>());
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

static void AddPerfomanceLogging(LoggerConfiguration loggerConfiguration, string logPath, string? restApiHost, string? settingsAPIHost, string logContextFile)
{
    loggerConfiguration.AddPerfomanceCounter(logContextFile, logPath, restApiHost, EventIds.Perfomance.Id, "AlchemyRestAPIClient");
    loggerConfiguration.AddPerfomanceCounter(logContextFile, logPath, settingsAPIHost, EventIds.Perfomance.Id, "AlchemySettingsRestAPIClient");

    loggerConfiguration.AddContextPropertyConfig(
        logContextFile,
        $"{logPath}/Perfomance",
        "Host",
        "Perfomance",
        [new PropertyExpression("=", [SerilogExpressions.EventId, EventIds.Perfomance.Id])],
        ["Url"]
        );
}

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder, IWebHostEnvironment environment,  string? restApiHost, string? settingsAPIHost)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    AddShopImportLogging(logPath, environment, loggerConfiguration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(ShopImportWorker).Name);

    AddPerfomanceLogging(loggerConfiguration, logPath, restApiHost, settingsAPIHost, logContextFile);

    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ImportBackgroundServiceProgram
{ }