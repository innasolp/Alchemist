using Alchemist.BrowserService.Client;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Service.Factory.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
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
using Message.Interfaces;
using Message.RabbitMQ.DependencyInjection;
using Serilog.Configuration.Extensions;
using Serilog.Loggers;
using WebLoader.Interfaces;

var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";


var builder = WebApplication.CreateBuilder(args);

AddSettingsAdapters(builder);

AddLoaderService(builder);

AddShopAPIService(builder, out var restApiHost);

AddShopSettingsAPIService(builder, out var settingsAPIHost);

builder.Services.AddPerfomanceCounter<RequestDelegatingHandler>((logger) => new SerilogUrlLogger<PerfomanceCounter<RequestDelegatingHandler>>(logger));

AddMessages(builder);

AddShopImporters(builder);

AddLogging(logPath, builder, restApiHost, settingsAPIHost);

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
    builder.Services.AddShopImportMessageSender(builder.Configuration, "SignalRImportUrl", "monitorItemSender");
    builder.Services.AddShopImportMessageSender(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageSenderKey);
    builder.Services.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageReceiverKey);
    
    builder.Services.CollectServicesToEnumerable<IMessageSender>(["monitorItemSender"], ShopImportWorkerKeys.ShopsMessageSenderKey);
}

static void AddSettingsAdapters(WebApplicationBuilder builder)
{
    builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, ImportServiceSettings>(Alchemist.Product.Interfaces.ShopSettingType.Product);
    builder.Services.AddSettingsDataAdapter<CategoryShopImportSettings, ImportServiceSettings>(Alchemist.Product.Interfaces.ShopSettingType.Category);
    builder.Services.AddSettingsJsonAdapter<ProductShopImportSettings>("shopProducts.json");
    builder.Services.AddSettingsJsonAdapter<CategoryShopImportSettings>("shopCategories.json");
}

static void AddLoaderService(WebApplicationBuilder builder)
{
    builder.Services.AddKeyedSingleton(nameof(BrowserServiceClientFactory),
           builder.Configuration.GetHostSectionValue("BrowserServiceHost").SetEnvironmentLocalHostIfNeed());
    builder.Services.AddSingleton<ILoaderServiceFactory, BrowserServiceClientFactory>();
    builder.Services.AddServiceImplementationsFromPath(typeof(IWebLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("WebLoaderPath").Value}");
}

static void AddShopAPIService(WebApplicationBuilder builder, out string restApiHost)
{
    restApiHost = builder.Configuration.GetHostSectionValue("RestAPIHost");
    builder.Services.ConfigureDefaultHttps();
    builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient), out var shopHttpClientBuilder);
    builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(shopHttpClientBuilder);
}

static void AddShopSettingsAPIService(WebApplicationBuilder builder, out string settingsAPIHost)
{
    settingsAPIHost = builder.Configuration.GetHostSectionValue("SettingsAPIHost");
    builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out var settingsHttpClientBuilder);
    builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(settingsHttpClientBuilder);
}

static void AddShopImporters(WebApplicationBuilder builder)
{
    builder.Services.AddServiceImplementationsFromPath(typeof(IShopImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopProductImportPath").Value}");
    builder.Services.AddServiceImplementationsFromPath(typeof(IShopImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopCategoryImportPath").Value}");

    builder.Services.AddImportServiceLogFactory((logger, name, shopModel, settings) => new SerilogPropertyLogger(logger, new Dictionary<string, object>{
    { "ShopImportService", name },
    { "ShopSettingsType", (settings as IShopSettings).Type.ToString() } }));

    builder.Services.AddPerfomanceCounter((logger) => new SerilogUrlLogger<IPerfomanceCounter>(logger));

    AddShopImportItemHandlers(builder.Services, builder.Configuration);
}

static void AddShopImportItemHandlers(IServiceCollection services, IConfiguration configuration)
{
    var rabbitMQOptions = configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
    rabbitMQOptions.RabbitMqServiceOptions.HostName = rabbitMQOptions.RabbitMqServiceOptions.HostName.SetEnvironmentLocalHostIfNeed();
    services.AddRabbitMQMessageSender("importqueue", rabbitMQOptions);
    services.AddProductItemHandler("importqueue", configuration.GetSection("RabbitMQProductEvent").Get<string>());
    services.AddCategoryItemHandler("importqueue", configuration.GetSection("RabbitMQCategoryEvent").Get<string>());
}

static void AddShopImportLogging(string logPath, IWebHostEnvironment environment, SerilogConfigurationBuilder appLogConfBuilder)
{
    appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{environment.ContentRootPath}/log.contextproperty.json",
        logPath: $"{logPath}/Import/Products",
        propertyName: "ShopImportService",
        sourceContext: "Import",
        null,
        [new SerilogPropertyExpression(SerilogFunc.Contains, [new ContextProperty("ShopSettingsType"), ShopSettingType.Product.ToString()])]);

    appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{environment.ContentRootPath}/log.contextproperty.json",
        logPath: $"{logPath}/Import/Categories",
        propertyName: "ShopImportService",
        sourceContext: "Import",
        null,
        [new SerilogPropertyExpression(SerilogFunc.Contains, [new ContextProperty("ShopSettingsType"), ShopSettingType.Category.ToString()])]);
}

static void AddPerfomanceLogging(string logPath, WebApplicationBuilder builder, string? restApiHost, string? settingsAPIHost, string logContextPath, SerilogConfigurationBuilder appLogConfBuilder)
{
    appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: "alchemygrpcservice", EventIds.Perfomance.Id, serviceName: "AlchemyGrpcClient");
    appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: restApiHost, EventIds.Perfomance.Id, serviceName: "AlchemyRestAPIClient");
    appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: settingsAPIHost, EventIds.Perfomance.Id, serviceName: "AlchemySettingsRestAPIClient");

    appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
        logPath: $"{logPath}/Perfomance",
        propertyName: "Host",
        sourceContext: "Perfomance",
        ["Url"],
        [new SerilogPropertyExpression("=", [SerilogExpressions.EventId, EventIds.Perfomance.Id]),
     new SerilogPropertyExpression("<>",[new ContextProperty("Host"), "localhost"])]);
}

static void AddLogging(string logPath, WebApplicationBuilder builder, string? restApiHost, string? settingsAPIHost)
{
    var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
    var appLogConfBuilder = new SerilogConfigurationBuilder(builder.Configuration);

    AddShopImportLogging(logPath, builder.Environment, appLogConfBuilder);

    appLogConfBuilder.AddServiceBaseConfigs(logContextPath, logPath, typeof(ShopImportWorker).Name);

    AddPerfomanceLogging(logPath, builder, restApiHost, settingsAPIHost, logContextPath, appLogConfBuilder);

    appLogConfBuilder.SetSerilog(builder.Logging);
}

public class ImportBackgroundServiceProgram
{ }