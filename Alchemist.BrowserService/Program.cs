using Alchemist.BrowserService.Controllers;
using Alchemist.Common;
using Alchemist.Log.Extensions;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using DependencyInjection.AssemblyExtensions;
using Http.ErrorHandling;
using Http.Info;
using Microsoft.OpenApi;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserDataLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserDataLoaderPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserLauncher), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserLauncherPath").Value}");


builder.Services.AddControllers();
builder.Services.AddAuthentication("https");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SwaggerOptions>(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler<BrowserServiceController>>();
builder.Services.AddSingleton<InfoLogMiddleware<BrowserServiceController>>();
builder.Services.AddProblemDetails();

AddLogging(builder.Configuration, builder.Logging);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<InfoLogMiddleware<BrowserServiceController>>();

app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello BrowserService!");

app.Run();

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.BrowserService";

    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class BrowserServiceProgramm { }
