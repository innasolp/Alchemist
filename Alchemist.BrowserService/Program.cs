using Alchemist.BrowserService.Controllers;
using Alchemist.Common;
using Alchemist.Log.Extensions;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using DependencyInjection.AssemblyExtensions;
using Http.ErrorHandling;
using Http.Info;
using Serilog.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserDataLoader), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserDataLoaderPath").Value}");
builder.Services.AddServiceImplementationsFromPath(typeof(IBrowserLauncher), $"{Utils.GetAppPath()}\\{builder.Configuration.GetSection("BrowserLauncherPath").Value}");


builder.Services.AddControllers();
builder.Services.AddAuthentication("https");
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler<BrowserServiceController>>();
builder.Services.AddSingleton<InfoLogMiddleware<BrowserServiceController>>();
builder.Services.AddProblemDetails();

var logPath = $"{Utils.GetAppPath()}/Logs";
var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
var appSerilogBuilder = new SerilogConfigurationBuilder(builder.Configuration);
var serviceName = "Alchemist.BrowserService";
appSerilogBuilder.AddServiceBaseConfigs(logContextPath, logPath, serviceName);
appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

appSerilogBuilder.SetSerilog(builder.Logging);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<InfoLogMiddleware<BrowserServiceController>>();

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

app.MapGet("/", () => "Hello BrowserService!");

app.Run();

public class BrowserServiceProgramm { }
