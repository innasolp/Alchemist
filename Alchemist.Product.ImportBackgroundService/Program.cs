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
using BrowserDataLoader.Interfaces;
using Alchemist.Import.Factory;
using DependencyInjection.AssemblyExtensions;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Import.Products.Data;
using Alchemist.Import.Categories.Data;
using Alchemist.Import.Logging;

var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient));
builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();
builder.Services.AddSettingsJsonAdapter("shopProducts.json", "shopCategories.json");

builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserDataLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserDataLoaderPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IWebLoaderFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("WebLoaderPath").Value}");

builder.Services.AddShopImportMessageSender(builder.Configuration, "SignalRImportUrl", ShopImportWorkerKeys.ShopsMessageSenderKey);
builder.Services.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl");

builder.Services.AddGrpcServiceClient<Alchemist.Product.GrpcServiceClient.AlchemyGrpcServiceClient>(builder.Configuration, "GrpcAPIHost");
builder.Services.AddSingleton<Interceptor, GrpcClientRequestInterceptor>();
builder.Services.AddPerfomanceCounter<Interceptor, GrpcClientRequestInterceptor>((logger) => new SerilogUrlLogger<PerfomanceCounter<GrpcClientRequestInterceptor>>(logger));


builder.Services.ConfigureDefaultHttps();
builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient));
var restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>()?.SetEnvironmentLocalHostIfNeed();
builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(restApiHost);
builder.Services.AddPerfomanceCounter<RequestDelegatingHandler>((logger) => new SerilogUrlLogger<PerfomanceCounter<RequestDelegatingHandler>>(logger));

 
builder.Services.AddServiceImplementationsFromPath(typeof(IShopImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopProductImportPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IShopImportServiceFactory), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("ShopCategoryImportPath").Value}");
builder.Services.AddProductDataHandler();
builder.Services.AddCategoriesDataHandler();
builder.Services.AddImportServiceLogFactory((logger, shopModel, settings) => new SerilogPropertyLogger(logger, "ClassName", shopModel.Name));

var appSerilogBuilder = new AppSerilogBuilder(builder.Configuration,builder.Environment);
appSerilogBuilder.AddServiceBaseConfigs(typeof(ShopImportWorker).Name);
appSerilogBuilder.AddPerfomanceCounter(url:"alchemygrpcservice", EventIds.Perfomance.Id, logPath, serviceName:"AlchemyGrpcClient");
appSerilogBuilder.AddPerfomanceCounter(url: restApiHost, EventIds.Perfomance.Id, logPath, serviceName:"AlchemyRestAPIClient");

//todo
//if (shopProductsSettings.Count != 0)
//{
//    appSerilogBuilder.AddShopsSerilogSourceContextConfigs(shopProductsSettings, $"{logPath}/Shops");
//    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopProductsSettings, logPath);
//}

//if (shopCategoriesSettings.Count != 0)
//{
//    appSerilogBuilder.AddShopsSerilogPropertyConfigs(shopCategoriesSettings, $"{logPath}/ShopCategories");
//    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopCategoriesSettings, logPath);
//}

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
