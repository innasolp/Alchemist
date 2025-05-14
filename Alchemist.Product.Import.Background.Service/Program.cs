using Alchemist.Common;
using Alchemist.Log.Serilog;
using Alchemist.Product.Import.Background;
using Alchemist.Product.RestAPIClient;
using Grpc.Core.Interceptors;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Serilog.Loggers;
using Grpc.Client.RequestInterceptor;
using Alchemist.DataService.Interfaces;
using Alchemist.Settings.RestAPIClient;
using Alchemist.DependencyInjection.Common;
using WebLoader.Interfaces;
using DependencyInjection.AssemblyExtensions;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Import.Products.Data;
using Alchemist.Import.Categories.Data;
using Alchemist.Import.Logging;
using Alchemist.Import.Settings.Model;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using BrowserDataLoader.Interfaces;
using Alchemist.Import.Factory.Interfaces;
using Serilog.Configuration.Extensions;

var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();
builder.Services.AddSettingsJsonAdapter<ProductShopImportSettings, CategoryShopImportSettings>("shopProducts.json", "shopCategories.json");

builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserDataLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserDataLoaderPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IWebLoaderFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("WebLoaderPath").Value}");

builder.Services.AddShopImportMessageSender(builder.Configuration, "SignalRImportUrl", ShopImportWorkerKeys.ShopsMessageSenderKey);
builder.Services.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl");

builder.Services.AddGrpcServiceClient<Alchemist.Product.GrpcServiceClient.AlchemyGrpcServiceClient>(builder.Configuration, "GrpcAPIHost");
builder.Services.AddSingleton<Interceptor, GrpcClientRequestInterceptor>();
builder.Services.AddPerfomanceCounter<Interceptor, GrpcClientRequestInterceptor>((logger) => new SerilogUrlLogger<PerfomanceCounter<GrpcClientRequestInterceptor>>(logger));


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
builder.Services.AddProductDataHandler();
builder.Services.AddCategoriesDataHandler();
builder.Services.AddImportServiceLogFactory((logger, shopModel, settings) => new SerilogPropertyLogger(logger, new Dictionary<string, object>{ 
    { "ShopImportService", shopModel.Name },
    { "ShopSettingsType", settings.ShopSettingType.ToString() } }));
builder.Services.AddPerfomanceCounter((logger) => new SerilogUrlLogger<IPerfomanceCounter>(logger));

var appSerilogBuilder = new AppSerilogBuilder(builder.Configuration, builder.Environment);
appSerilogBuilder.AddServiceBaseConfigs(typeof(ShopImportWorker).Name);
appSerilogBuilder.AddPerfomanceCounter(url:"alchemygrpcservice", EventIds.Perfomance.Id, logPath, serviceName:"AlchemyGrpcClient");
appSerilogBuilder.AddPerfomanceCounter(url: restApiHost, EventIds.Perfomance.Id, logPath, serviceName:"AlchemyRestAPIClient");
appSerilogBuilder.AddPerfomanceCounter(url: settingsAPIHost, EventIds.Perfomance.Id, logPath, serviceName:"AlchemySettingsRestAPIClient");

appSerilogBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Import/Products",
    propertyName: "ShopImportService", 
    sourceContext: "Import",
    null,
    [ new SerilogPropertyExpression(SerilogExpressions.Contains, "ShopSettingsType", ShopSettingType.Product.ToString()) ]);

appSerilogBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Import/Categories",
    propertyName: "ShopImportService",
    sourceContext: "Import",
    null,
    [new SerilogPropertyExpression(SerilogExpressions.Contains, "ShopSettingsType", ShopSettingType.Category.ToString())]);

appSerilogBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Perfomance",
    propertyName: "Host",
    sourceContext: "Perfomance",
    ["Url"],
    [new SerilogPropertyExpression(SerilogExpressions.EventId, EventIds.Perfomance.Id)]);

appSerilogBuilder.SetSerilog(builder.Logging);

builder.Services.AddHostedService<ShopImportWorker>();

builder.Services.AddAuthentication("https");

builder.WebHost.UseUrls("http://localhost:8130", "https://localhost:8131");

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
