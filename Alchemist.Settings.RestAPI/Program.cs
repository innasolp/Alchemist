using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Log.Extensions;
using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Settings.Data.Repository;
using Alchemist.Settings.RestAPI.Controllers;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Http.ErrorHandling;
using Http.Info;
using Message.SignalR.HubMessage.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

// Add services to the container.

builder.Services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")));

builder.Services.AddScoped<ISettingsRepository,SettingsRepository>();

var signalRUrl = builder.Configuration.GetSection("SignalRUrl").Get<string>();
builder.Services.AddSignalRHubMessageSender(signalRUrl);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("https");

builder.Services.AddExceptionHandler<GlobalExceptionHandler<SettingsController>>();
builder.Services.AddSingleton<InfoLogMiddleware<SettingsController>>();
builder.Services.AddProblemDetails();

var logPath = $"{Utils.GetAppPath()}/Logs";
var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
var appSerilogBuilder = new SerilogConfigurationBuilder(builder.Configuration);
var serviceName = "Alchemist.Settings.RestAPI";
appSerilogBuilder.AddServiceBaseConfigs(logContextPath, logPath, serviceName);
appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

appSerilogBuilder.SetSerilog(builder.Logging);


var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<InfoLogMiddleware<SettingsController>>();

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

app.Run();

public class SettingsAPIProgram { }
