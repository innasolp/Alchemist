using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Data.Repository;
using Alchemist.Product.RestAPI.Controllers;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Http.ErrorHandling;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;
using Message.SignalR.HubMessage.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Loggers;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();


// Add services to the container.

builder.Services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")));

builder.Services.AddScoped<IAlchemyRepository, AlchemyRepository>();

var signalRUrl = builder.Configuration.GetSection("SignalRUrl").Get<string>();
builder.Services.AddSignalRHubMessageSender(signalRUrl);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("https");

builder.Services.AddExceptionHandler<GlobalExceptionHandler<ShopController>>();
builder.Services.AddSingleton<InfoLogMiddleware<ShopController>>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler<ShopCategoryController>>();

builder.Services.AddProblemDetails();

builder.Services.AddPerfomanceCounter<InfoLogMiddleware<ShopController>>((logger) => new SerilogUrlLogger<PerfomanceCounter<InfoLogMiddleware<ShopController>>>(logger));

AddLogging(builder.Configuration, builder.Logging);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<InfoLogMiddleware<ShopController>>();

(app as IHost).UsePerfomanceCounters();

app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });
}

app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.Shop.RestAPI";

    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    loggerConfiguration.AddPerfomanceCounter(logContextFile, logPath, url: "https://localhost:8051", EventIds.Perfomance.Id, serviceName);
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ShopAPIProgram { }
