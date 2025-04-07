using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Log.Serilog;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Settings.Data.Repository;
using Alchemist.Settings.RestAPI.Controllers;
using Http.ErrorHandling;
using Http.Info;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContextFactory<AlchemyContextPostgres>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")?
    .SetEnvironmentLocalHostIfNeed()));

builder.Services.AddScoped<ISettingsRepository,SettingsRepository>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("https");

builder.Services.AddExceptionHandler<GlobalExceptionHandler<SettingsController>>();
builder.Services.AddSingleton<InfoLogMiddleware<SettingsController>>();
builder.Services.AddProblemDetails();

var logPath = $"{Utils.GetAppPath()}/Logs";
var appSerilogBuilder = new AppSerilogBuilder(builder);
var serviceName = "Alchemist.Settings.RestAPI";
appSerilogBuilder.AddServiceBaseConfigs(serviceName);
appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
appSerilogBuilder.AddSourceContextLogConfig($"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());

appSerilogBuilder.SetSerilog();


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
