using Alchemist.Common;
using Alchemist.Import.Settings.Model;
using Alchemist.Log.Serilog;
using Alchemist.Product.Import.Background;
using Alchemist.Product.RestAPIClient;
using Grpc.Client.RequestInterceptor;
using Grpc.Core.Interceptors;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Json.Extensions;
using Serilog.Loggers;

var shopProductsJsonFile = "shopProducts.json";
var shopCategoriesJsonFile = "shopCategories.json";

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

var appPath = Utils.GetAppPath();

var shopProductsSettings = await shopProductsJsonFile.ReadFromJsonFileAsync<ProductShopImportSettings[]>();
if (shopProductsSettings != null)
{
    foreach (var item in shopProductsSettings)
        item.SetAppPath(appPath);

    shopImportWorkerBuilder.AddShopProducts(shopProductsSettings);
    shopImportWorkerBuilder.AddProductsHandler();
}

var shopCategoriesSettings = await shopCategoriesJsonFile.ReadFromJsonFileAsync<ShopImportSettings[]>();
if (shopCategoriesSettings != null)
{
    foreach (var item in shopCategoriesSettings)
        item.SetAppPath(appPath);

    shopImportWorkerBuilder.AddShopCategories(shopCategoriesSettings);
    shopImportWorkerBuilder.AddPropertyValueInterceptorsLogging(shopCategoriesSettings, "ClassName", (shopSetting) => shopSetting.Id);
    shopImportWorkerBuilder.AddCategoriesHandler();
}

shopImportWorkerBuilder.AddShopImportMessageSender("SignalRImportUrl");
shopImportWorkerBuilder.AddShopImportDataReceiver("SignalREventsUrl");


var logPath = $"{appPath}/Logs";
var appSerilogBuilder = new AppSerilogBuilder(builder);
appSerilogBuilder.AddServiceBaseConfigs(typeof(ShopImportWorker).Name);
appSerilogBuilder.AddPerfomanceCounter(url:"alchemygrpcservice", EventIds.Perfomance.Id, logPath, serviceName:"AlchemyGrpcClient");
appSerilogBuilder.AddPerfomanceCounter(url: restApiHost, EventIds.Perfomance.Id, logPath, serviceName:"AlchemyRestAPIClient");

if (shopProductsSettings != null)
{
    appSerilogBuilder.AddShopsSerilogSourceContextConfigs(shopProductsSettings, $"{logPath}/Shops");
    appSerilogBuilder.AddShopsWebPerfomanceConfigs(shopProductsSettings, logPath);
}

if (shopCategoriesSettings != null)
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
