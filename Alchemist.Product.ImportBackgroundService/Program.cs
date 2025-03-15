using Alchemist.Common;
using Alchemist.Log.Serilog;
using Alchemist.Product.Import.Background;
using Alchemist.Product.RestAPIClient;
using Grpc.Client.RequestInterceptor;
using Grpc.Core.Interceptors;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Serilog.Loggers;
using Alchemist.Import.Settings.Builders;
using Alchemist.Import.Settings.Interfaces;

var shopProductsJsonFile = "shopProducts.json";
var shopCategoriesJsonFile = "shopCategories.json";
var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";

var settingsHostBuilder = WebApplication.CreateBuilder(args);
var settingsSerilogBuilder = new AppSerilogBuilder(settingsHostBuilder);
var settingsBuilders = new ISettingsBuilder[]
{
    new ShopSettingsAppBuilder(settingsHostBuilder,"SettingsAPIHost", "RestAPIHost"),
    new ShopSettingsJsonBuilder(shopProductsJsonFile,shopCategoriesJsonFile)
};
settingsSerilogBuilder.AddSourceContextLogConfig($"{logPath}/ImportBackgroundService", nameof(ShopSettingsAppBuilder));
settingsSerilogBuilder.AddSourceContextLogConfig($"{logPath}/ImportBackgroundService", nameof(ShopSettingsJsonBuilder));

settingsSerilogBuilder.SetSerilog();
using var settingsHost = settingsHostBuilder.Build();
var shopImportData = await settingsBuilders[0].Build(settingsHost);
if (shopImportData.Count == 0 || shopImportData.All(s => s.ProductShopImportSettings == null && s.CategoryShopImportSettings == null))
    shopImportData = await settingsBuilders[1].Build(settingsHost);

foreach (var shop in shopImportData)
{
    shop.ProductShopImportSettings?.SetAppPath(appPath);
    shop.CategoryShopImportSettings?.SetAppPath(appPath);
}


var builder = WebApplication.CreateBuilder(args);

var shopImportWorkerBuilder = new ShopImportWorkerBuilder(builder);

shopImportWorkerBuilder.ConfigureDefaultHttps();
shopImportWorkerBuilder.AddRestApiClient<ShopApiClient>("RestAPIHost");
var restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>()?.SetEnvironmentLocalHostIfNeed();
shopImportWorkerBuilder.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(restApiHost);
builder.Services.AddPerfomanceCounter(typeof(RequestDelegatingHandler), (logger) => new SerilogUrlLogger(logger));

shopImportWorkerBuilder.AddGrpcServiceClient<Alchemist.Product.GrpcServiceClient.AlchemyGrpcServiceClient>("GrpcAPIHost");
builder.Services.AddSingleton<Interceptor, GrpcClientRequestInterceptor>();
builder.Services.AddPerfomanceCounter(typeof(Interceptor), typeof(GrpcClientRequestInterceptor), (logger) => new SerilogUrlLogger(logger));

shopImportWorkerBuilder.AddShopProducts(shopImportData);
shopImportWorkerBuilder.AddProductsHandler();

var shopCategoriesSettings = shopImportData.Select(s => s.CategoryShopImportSettings).ToList();

if (shopCategoriesSettings.Count > 0)
{
    shopImportWorkerBuilder.AddShopCategories(shopImportData);
    shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(shopCategoriesSettings.OfType<IShopImportSettings>().ToArray(), "ClassName", (shopSetting) => shopSetting.Id);
    shopImportWorkerBuilder.AddCategoriesHandler();
}


shopImportWorkerBuilder.AddShopImportMessageSender("SignalRImportUrl");
shopImportWorkerBuilder.AddShopImportDataReceiver("SignalREventsUrl");


var appSerilogBuilder = new AppSerilogBuilder(builder);
appSerilogBuilder.AddServiceBaseConfigs(typeof(ShopImportWorker).Name);
appSerilogBuilder.AddPerfomanceCounter(url:"alchemygrpcservice", EventIds.Perfomance.Id, logPath, serviceName:"AlchemyGrpcClient");
appSerilogBuilder.AddPerfomanceCounter(url: restApiHost, EventIds.Perfomance.Id, logPath, serviceName:"AlchemyRestAPIClient");

if (shopImportData.Count != 0)
{
    var shopProductsSettings = shopImportData.Select(s => s.ProductShopImportSettings).ToList();
    appSerilogBuilder.AddShopsSerilogSourceContextConfigs(shopProductsSettings, $"{logPath}/Shops");
    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopProductsSettings, logPath);
}

if (shopCategoriesSettings.Count != 0)
{
    appSerilogBuilder.AddShopsSerilogPropertyConfigs(shopCategoriesSettings, $"{logPath}/ShopCategories");
    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopCategoriesSettings, logPath);
}

appSerilogBuilder.SetSerilog();

builder.Services.AddHostedService<ShopImportWorker>();

builder.Services.AddAuthentication("https");

var app = builder.Build();

app.UseAuthentication();

//app.UseHsts();

app.UseHttpsRedirection();

app.UsePerfomanceCounters();

//app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.Run();
