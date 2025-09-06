using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Log.Extensions;
using Alchemist.Product.Import.Background;
using Alchemist.Product.Import.Background.Settings;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using DependencyInjection.AssemblyExtensions;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Serilog.Configuration.Extensions;
using Serilog.Loggers;
using WebLoader.Interfaces;
using Message.RabbitMQ.DependencyInjection;
using Alchemist.Import.Service.Factory.Interfaces;
using Alchemist.BrowserService.Client;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;

var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, ImportServiceSettings>(Alchemist.Product.Interfaces.ShopSettingType.Product);
builder.Services.AddSettingsDataAdapter<CategoryShopImportSettings, ImportServiceSettings>(Alchemist.Product.Interfaces.ShopSettingType.Category);
builder.Services.AddSettingsJsonAdapter<ProductShopImportSettings>("shopProducts.json");
builder.Services.AddSettingsJsonAdapter<CategoryShopImportSettings>("shopCategories.json");

builder.Services.AddKeyedSingleton(nameof(BrowserServiceClientFactory),
       builder.Configuration.GetHostSectionValue("BrowserServiceHost").SetEnvironmentLocalHostIfNeed());
builder.Services.AddSingleton<ILoaderServiceFactory, BrowserServiceClientFactory>();

builder.Services.AddServiceImplementationsFromPath(typeof(IWebLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("WebLoaderPath").Value}");

builder.Services.AddShopImportMessageSender(builder.Configuration, "SignalRImportUrl", ShopImportWorkerKeys.ShopsMessageSenderKey);
builder.Services.AddShopImportMessageSender(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageSenderKey);
builder.Services.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl", ShopImportWorkerKeys.EventMessageReceiverKey);


builder.Services.ConfigureDefaultHttps();
builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient), out var shopHttpClientBuilder);
var restApiHost = builder.Configuration.GetHostSectionValue("RestAPIHost");
builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(shopHttpClientBuilder);

builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out var settingsHttpClientBuilder);
var settingsAPIHost = builder.Configuration.GetHostSectionValue("SettingsAPIHost");
builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(settingsHttpClientBuilder);

builder.Services.AddPerfomanceCounter<RequestDelegatingHandler>((logger) => new SerilogUrlLogger<PerfomanceCounter<RequestDelegatingHandler>>(logger));


builder.Services.AddServiceImplementationsFromPath(typeof(IShopImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopProductImportPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IShopImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopCategoryImportPath").Value}");

//todo rabbitmqpublisher
var rabbitMQOptions = builder.Configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
rabbitMQOptions.RabbitMqServiceOptions.HostName = rabbitMQOptions.RabbitMqServiceOptions.HostName.SetEnvironmentLocalHostIfNeed();
builder.Services.AddRabbitMQMessageSender("importqueue", rabbitMQOptions);
builder.Services.AddProductItemHandler("importqueue", builder.Configuration.GetSection("RabbitMQProductEvent").Get<string>());
builder.Services.AddCategoryItemHandler("importqueue", builder.Configuration.GetSection("RabbitMQCategoryEvent").Get<string>());

builder.Services.AddImportServiceLogFactory((logger, shopModel, settings) => new SerilogPropertyLogger(logger, new Dictionary<string, object>{ 
    { "ShopImportService", settings.Name },
    { "ShopSettingsType", settings.ShopSettingType.ToString() } }));
builder.Services.AddPerfomanceCounter((logger) => new SerilogUrlLogger<IPerfomanceCounter>(logger));

var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
var appLogConfBuilder = new SerilogConfigurationBuilder(builder.Configuration);
appLogConfBuilder.AddServiceBaseConfigs(logContextPath, logPath, typeof(ShopImportWorker).Name);
appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: "alchemygrpcservice", EventIds.Perfomance.Id, serviceName:"AlchemyGrpcClient");
appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: restApiHost, EventIds.Perfomance.Id, serviceName:"AlchemyRestAPIClient");
appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: settingsAPIHost, EventIds.Perfomance.Id, serviceName:"AlchemySettingsRestAPIClient");

appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Import/Products",
    propertyName: "ShopImportService", 
    sourceContext: "Import",
    null,
    [ new SerilogPropertyExpression(SerilogFunc.Contains, [new ContextProperty("ShopSettingsType"), ShopSettingType.Product.ToString()]) ]);

appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Import/Categories",
    propertyName: "ShopImportService",
    sourceContext: "Import",
    null,
    [new SerilogPropertyExpression(SerilogFunc.Contains, [new ContextProperty("ShopSettingsType"), ShopSettingType.Category.ToString()])]);

appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Perfomance",
    propertyName: "Host",
    sourceContext: "Perfomance",
    ["Url"],
    [new SerilogPropertyExpression("=", [SerilogExpressions.EventId, EventIds.Perfomance.Id]), 
     new SerilogPropertyExpression("<>",[new ContextProperty("Host"), "localhost"])]);

appLogConfBuilder.SetSerilog(builder.Logging);

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

public class ImportBackgroundServiceProgram
{ }
