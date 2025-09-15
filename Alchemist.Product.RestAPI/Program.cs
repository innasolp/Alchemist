using Alchemist.Common;
using Alchemist.Product.Data.Repository;
using Alchemist.Log.Extensions;
using Http.ErrorHandling;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.EntityFrameworkCore;
using Serilog.Loggers;
using Alchemist.Product.Data;
using Alchemist.Product.RestAPI.Controllers;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data.Postgresql;
using Serilog.Configuration.Extensions;
using Message.SignalR.HubMessage.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")?
    .SetEnvironmentLocalHostIfNeed()));

builder.Services.AddScoped<IAlchemyRepository, AlchemyRepository>();

var signalRUrl = builder.Configuration.GetSection("SignalRUrl").Get<string>()?.SetEnvironmentLocalHostIfNeed();
builder.Services.AddSignalRHubMessageSender(signalRUrl);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("https");

builder.Services.AddExceptionHandler<GlobalExceptionHandler<ShopController>>();
builder.Services.AddSingleton<InfoLogMiddleware<ShopController>>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler<ShopCategoryController>>();
builder.Services.AddSingleton<InfoLogMiddleware<ShopCategoryController>>();

builder.Services.AddProblemDetails();

builder.Services.AddPerfomanceCounter<InfoLogMiddleware<ShopController>>((logger) => new SerilogUrlLogger<PerfomanceCounter<InfoLogMiddleware<ShopController>>>(logger));



var logPath = $"{Utils.GetAppPath()}/Logs";
var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
var appSerilogBuilder = new SerilogConfigurationBuilder(builder.Configuration);
var serviceName = "Alchemist.Shop.RestAPI";
appSerilogBuilder.AddServiceBaseConfigs(logContextPath, logPath, serviceName);
appSerilogBuilder.AddPerfomanceCounter(logContextPath, logPath, url: "https://localhost:8051", EventIds.Perfomance.Id, serviceName);
appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

appSerilogBuilder.SetSerilog(builder.Logging);


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

public class ShopAPIProgram { }
