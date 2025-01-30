using Alchemist.Common;
using Alchemist.Log.Serilog;
using Alchemist.Product.Import.Background;
using Alchemist.Product.RestAPIClient;
using Grpc.Client.RequestInterceptor;
using Grpc.Core.Interceptors;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
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
shopImportWorkerBuilder.AddSingleton<Interceptor, GrpcClientRequestInterceptor>();
builder.Services.AddPerfomanceCounter(typeof(Interceptor), typeof(GrpcClientRequestInterceptor), (logger) => new SerilogUrlLogger(logger));

var appPath = Utils.GetAppPath();

var shopProductsSettings = shopProductsJsonFile.GetSettings<ShopProductsSettings[]>("ShopProducts");
if (shopProductsSettings != null)
{
    shopImportWorkerBuilder.AddShopProducts(shopProductsSettings, appPath);
    shopImportWorkerBuilder.AddProductsHandler();
}

var shopCategoriesSettings = shopCategoriesJsonFile.GetSettings<ShopCategoriesSettings[]>("ShopCategories");
if (shopCategoriesSettings != null)
{
    shopImportWorkerBuilder.AddShopCategories(shopCategoriesSettings, appPath);
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
