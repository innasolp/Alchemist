using Alchemist.Common;
using Alchemist.DependencyInjection.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.BeautyAndHealth;
using Alchemist.Product.BeautyAndHealth.Commands;
using Alchemist.Product.CategoryData;
using Alchemist.Product.Import.DBService;
using Alchemist.Product.Interfaces;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Db.Infrastructure.Commands;
using Message.RabbitMQ.DependencyInjection;
using Serilog;
using Shop.API.Client;
using Shop.Import.Category.Commands;
using Shop.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();


builder.Services.AddGrpcServiceClient<IProductDataService, Alchemist.Product.GrpcServiceClient.AlchemyGrpcServiceClient>(builder.Configuration, "GrpcAPIHost");

builder.Services.ConfigureDefaultHttps();
builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "RestAPIHost", nameof(ShopApiClient), out var shopHttpClientBuilder);
var restApiHost = builder.Configuration.GetSection("RestAPIHost").Get<string>();

var rabbitMQOptions = builder.Configuration.GetRabbitMQOptions("RabbitMqServiceOptions", "RabbitMqQueueOptions", "RabbitMqExchangeOptions");
builder.Services.AddRabbitMQMessageReceiver(rabbitMQOptions);

builder.Services.AddKeyedSingleton<IDictionary<string, Type>>("ImportEvents", new Dictionary<string,Type>
    { {builder.Configuration.GetSection("RabbitMQProductEvent").Get<string>(), typeof(ImportBeautyAndHealthProductCommand) },
    { builder.Configuration.GetSection("RabbitMQCategoryEvent").Get<string>(), typeof(ImportShopCategoryCommand) }
});


builder.Host.AddShopCategoryImportInfrastructure();
builder.Host.AddBeautyAndHealthImportInfrastructure();

builder.Services.AddCommandHandlerFactory();


AddLogging(builder.Configuration, builder.Logging, "log.property.json", $"{Utils.GetAppPath()}/Logs", restApiHost);

builder.Services.AddHostedService<ImportItemHandlerService>();

builder.Services.AddAuthentication("https");

//builder.WebHost.UseUrls("http://localhost:8230", "https://localhost:8231");


var app = builder.Build();

app.UseAuthentication();

//app.UseHsts();

app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseRouting();


app.MapGet("/", () => "Hello ImportDBService!");

await app.RunAsync();

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder, string logContextFile, string logPath, string? restApiHost)
{    
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, typeof(ImportItemHandlerService).Name);
    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ImportDbServiceProgram
{ }