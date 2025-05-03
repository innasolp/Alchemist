using Alchemist.Common;
using Alchemist.Log.Serilog;
using Alchemist.Product.Import.Background;
using Alchemist.Product.RestAPIClient;
using Grpc.Client.RequestInterceptor;
using Grpc.Core.Interceptors;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Serilog.Loggers;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Background.Service;

var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";


var builder = WebApplication.CreateBuilder(args);

var allShopSettings = await ShopSettingsContainerBuilder.BuildAsync(args, builder.Configuration, logPath, "shopProducts.json", "shopCategories.json");

foreach (var shop in allShopSettings)
{
    shop.ProductShopImportSettings?.SetAppPath(appPath);
    shop.CategoryShopImportSettings?.SetAppPath(appPath);
}

var shopImportWorkerBuilder = new ShopImportWorkerBuilder(builder.Services);

shopImportWorkerBuilder.ConfigureDefaultHttps();
shopImportWorkerBuilder.AddRestApiClient<ShopApiClient>(builder.Configuration, "RestAPIHost");
var restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>()?.SetEnvironmentLocalHostIfNeed();
shopImportWorkerBuilder.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(restApiHost);
builder.Services.AddPerfomanceCounter<RequestDelegatingHandler>((logger) => new SerilogUrlLogger<PerfomanceCounter<RequestDelegatingHandler>>(logger));

shopImportWorkerBuilder.AddGrpcServiceClient<Alchemist.Product.GrpcServiceClient.AlchemyGrpcServiceClient>(builder.Configuration, "GrpcAPIHost");
builder.Services.AddSingleton<Interceptor, GrpcClientRequestInterceptor>();
builder.Services.AddPerfomanceCounter<Interceptor, GrpcClientRequestInterceptor>((logger) => new SerilogUrlLogger<PerfomanceCounter<GrpcClientRequestInterceptor>>(logger));

var shopProductsSettings = allShopSettings.Where(s=>s.ProductShopImportSettings != null).Select(s => s.ProductShopImportSettings).ToList();
shopImportWorkerBuilder.AddShopProducts(shopProductsSettings);
shopImportWorkerBuilder.AddProductsHandler();

var shopCategoriesSettings = allShopSettings.Where(s => s.CategoryShopImportSettings != null).Select(s => s.CategoryShopImportSettings).ToList();

if (shopCategoriesSettings.Count > 0)
{
    shopImportWorkerBuilder.AddShopCategories(shopCategoriesSettings);
    shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(shopCategoriesSettings.OfType<IShopImportSettings>().ToArray(), "ClassName", (shopSetting) => shopSetting.Id);
    shopImportWorkerBuilder.AddCategoriesHandler();
}

shopImportWorkerBuilder.AddShopImportMessageSender(builder.Configuration, "SignalRImportUrl");
shopImportWorkerBuilder.AddShopImportDataReceiver(builder.Configuration, "SignalREventsUrl");


var appSerilogBuilder = new AppSerilogBuilder(builder);
appSerilogBuilder.AddServiceBaseConfigs(typeof(ShopImportWorker).Name);
appSerilogBuilder.AddPerfomanceCounter(url:"alchemygrpcservice", EventIds.Perfomance.Id, logPath, serviceName:"AlchemyGrpcClient");
appSerilogBuilder.AddPerfomanceCounter(url: restApiHost, EventIds.Perfomance.Id, logPath, serviceName:"AlchemyRestAPIClient");

if (shopProductsSettings.Count != 0)
{
    appSerilogBuilder.AddShopsSerilogSourceContextConfigs(shopProductsSettings, $"{logPath}/Shops");
    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopProductsSettings, logPath);
}

if (shopCategoriesSettings.Count != 0)
{
    appSerilogBuilder.AddShopsSerilogPropertyConfigs(shopCategoriesSettings, $"{logPath}/ShopCategories");
    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopCategoriesSettings, logPath);
}

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
