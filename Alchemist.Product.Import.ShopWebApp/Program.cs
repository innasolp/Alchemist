using Alchemist.Common;
using Alchemist.DependencyInjection.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.ShopWebApp.Controllers;
using Alchemist.WebApp.Api.Common;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Http.ErrorHandling;
using Serilog;
using Shop.API.Client;
using Shop.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

bool isApi = builder.IsApi(args);

builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "ShopAPIHost", nameof(ShopApiClient), out IHttpClientBuilder shopHttpClientBuilder);

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


if (isApi)
    builder.Services.AddSwaggerApi();

if (isApi)
    builder.Services.AddBaseControllerInterceptors();

var logger = AddLogging(builder.Configuration, builder.Logging, isApi);
builder.Host.UseSerilog(logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else if (isApi)
    app.UseBaseInterceptors();

if (isApi)
    app.UseApiSwagger();

app.UseHttpsRedirection();

// подключаем файлы по умолчанию
app.UseDefaultFiles();
// подключаем статические файлы
app.UseStaticFiles();

app.UseSerilogRequestLogging();

//todo later
//IHostEnvironment? env = app.Services.GetService<IHostEnvironment>();
//if (env != null)
//{
//    // добавляем поддержку каталога node_modules
//    app.UseFileServer(new FileServerOptions()
//    {
//        FileProvider = new PhysicalFileProvider(
//            Path.Combine(env.ContentRootPath, "node_modules")
//        ),
//        RequestPath = "/node_modules",
//        EnableDirectoryBrowsing = false
//    });
//}

app.UseRouting();

app.UseAuthorization();
if (isApi)
    app.SetApiRoute("Hello ShopWebApp API!");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static Serilog.ILogger AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder, bool isApi)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.Product.ShopWebApp";
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    if (isApi)
    {
        loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}/Http", "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware");
        loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", nameof(GlobalExceptionHandler));
    }
    return loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ShopWebAppProgram
{ }
