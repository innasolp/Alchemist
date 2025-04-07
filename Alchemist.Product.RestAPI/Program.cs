using Alchemist.Common;
using Alchemist.Product.Data.Repository;
using Alchemist.Log.Serilog;
using Http.ErrorHandling;
using Http.Info;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.EntityFrameworkCore;
using Serilog.Loggers;
using Alchemist.Product.Data;
using Alchemist.Product.RestAPI.Controllers;
using Message.SignalR.DependencyInjection;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Data.Postgresql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContextFactory<AlchemyContextPostgres>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")?
    .SetEnvironmentLocalHostIfNeed()));

builder.Services.AddScoped<IAlchemyRepository, AlchemyRepository>();

var signalRUrl = builder.Configuration.GetSection("SignalRUrl").Get<string>()?.SetEnvironmentLocalHostIfNeed();
builder.Services.AddSignalRMessageSender(signalRUrl);

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

builder.Services.AddPerfomanceCounter(typeof(InfoLogMiddleware<ShopController>), (logger) => new SerilogUrlLogger(logger));
builder.Services.AddPerfomanceCounter(typeof(InfoLogMiddleware<ShopCategoryController>), (logger) => new SerilogUrlLogger(logger));


var logPath = $"{Utils.GetAppPath()}/Logs";
var appSerilogBuilder = new AppSerilogBuilder(builder);
var serviceName = "Alchemist.Shop.RestAPI";
appSerilogBuilder.AddServiceBaseConfigs(serviceName);
appSerilogBuilder.AddPerfomanceCounter(url: "https://localhost:8051", EventIds.Perfomance.Id, logPath, serviceName);
appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

appSerilogBuilder.SetSerilog();


var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<InfoLogMiddleware<ShopController>>();
app.UseMiddleware<InfoLogMiddleware<ShopCategoryController>>();

app.UsePerfomanceCounters();

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
