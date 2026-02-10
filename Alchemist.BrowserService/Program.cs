using Alchemist.Common;
using Alchemist.Log.Extensions;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using DependencyInjection.AssemblyExtensions;
using Serilog;
using Alchemist.WebApp.Api.Common;
using Http.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserDataLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserDataLoaderPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserLauncher), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserLauncherPath").Value}");


builder.Services.AddControllers();
builder.Services.AddAuthentication("https");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerApi();

builder.Services.AddBaseControllerInterceptors();

var logger = AddLogging(builder.Configuration, builder.Logging);
builder.Host.UseSerilog(logger);

var app = builder.Build();

app.UseBaseInterceptors();

app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.UseApiSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSerilogRequestLogging();

app.SetApiRoute("Hello BrowserService!");

app.Run();

static Serilog.ILogger AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.BrowserService";

    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware");
    loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", nameof(GlobalExceptionHandler));

    return loggerConfiguration.SetSerilog(loggingBuilder);
}

public class BrowserServiceProgramm { }
