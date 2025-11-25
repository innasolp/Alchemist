using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.DbItemHandler;
using Alchemist.Product.Import.DBService;
using Alchemist.Product.RestAPIClient;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Grpc.Client.RequestInterceptor;
using Grpc.Core.Interceptors;
using Http.DelegatingRequestSender;
using Http.RequestHandling.PerfomanceCounter;
using Message.RabbitMQ.DependencyInjection;
using Serilog.Configuration.Extensions;
using Serilog.Loggers;

var appPath = Utils.GetAppPath();
var logPath = $"{appPath}/Logs";


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();


builder.Services.AddGrpcServiceClient<IProductDataService,Alchemist.Product.GrpcServiceClient.AlchemyGrpcServiceClient>(builder.Configuration, "GrpcAPIHost");
builder.Services.AddSingleton<Interceptor, GrpcClientRequestInterceptor>();
builder.Services.AddPerfomanceCounter<Interceptor, GrpcClientRequestInterceptor>((logger) => new SerilogUrlLogger<PerfomanceCounter<GrpcClientRequestInterceptor>>(logger));

builder.Services.ConfigureDefaultHttps();
builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient), out var shopHttpClientBuilder);
var restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>();
builder.Services.AddHttpMessageDelegatingHandler<RequestDelegatingHandler>(shopHttpClientBuilder);

builder.Services.AddPerfomanceCounter<RequestDelegatingHandler>((logger) => new SerilogUrlLogger<PerfomanceCounter<RequestDelegatingHandler>>(logger));

var rabbitMQOptions = builder.Configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
builder.Services.AddRabbitMQMessageReceiver(rabbitMQOptions);

builder.Services.AddProductItemHandler(builder.Configuration.GetSection("RabbitMQProductEvent").Get<string>());
builder.Services.AddCategoryItemHandler(builder.Configuration.GetSection("RabbitMQCategoryEvent").Get<string>());

var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
var appLogConfBuilder = new SerilogConfigurationBuilder(builder.Configuration);
appLogConfBuilder.AddServiceBaseConfigs(logContextPath, logPath, typeof(ImportItemHandlerService).Name);
appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: "alchemygrpcservice", EventIds.Perfomance.Id, serviceName: "AlchemyGrpcClient");
appLogConfBuilder.AddPerfomanceCounter(logContextPath, logPath, url: restApiHost, EventIds.Perfomance.Id, serviceName: "AlchemyRestAPIClient");

appLogConfBuilder.AddContextPropertyConfig(logContextPath: $"{builder.Environment.ContentRootPath}/log.contextproperty.json",
    logPath: $"{logPath}/Perfomance",
    propertyName: "Host",
    sourceContext: "Perfomance",
    ["Url"],
    [new SerilogPropertyExpression("=", [SerilogExpressions.EventId, EventIds.Perfomance.Id]),
     new SerilogPropertyExpression("<>",[new ContextProperty("Host"), "localhost"])]);

appLogConfBuilder.SetSerilog(builder.Logging);

builder.Services.AddHostedService<ImportItemHandlerService>();

builder.Services.AddAuthentication("https");

builder.WebHost.UseUrls("http://localhost:8230", "https://localhost:8231");


var app = builder.Build();

app.UseAuthentication();

//app.UseHsts();

app.UseHttpsRedirection();

(app as IHost).UsePerfomanceCounters();

//app.UseAuthorization();

app.UseRouting();


app.MapGet("/", () => "Hello ImportDBService!");

await app.RunAsync();

public class ImportDbServiceProgram
{ }