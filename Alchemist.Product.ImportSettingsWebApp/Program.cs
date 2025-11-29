using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Log.Extensions;
using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchemist.Settings.RestAPIClient;
using Alchemist.WebApp.Api.Common;
using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Http.ErrorHandling;
using Http.Info;
using Serilog;
using Serilog.Configuration.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetAppSettingsCustomJsonConfigurationProvider();
builder.Configuration.AddCustomConfigurationRule<CustomJsonConfigurationSource, EnvironmentConfigurationRule>();

bool isApi = builder.IsApi(args);

builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out IHttpClientBuilder settingsHttpClientBuilder);
builder.Services.AddKeyedTypedSettingsDataAdapter<ProductShopImportSettingsModel, ServiceSettingsModel>(ShopSettingType.Product, ShopSettingType.Product);
builder.Services.AddKeyedTypedSettingsDataAdapter<CategoryShopImportSettingsModel, ServiceSettingsModel>(ShopSettingType.Category, ShopSettingType.Category);

if(!isApi)
    builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ShopImportSettingsApp.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.IsEssential = true;
});

if (isApi)
{
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerApi();
}

if (isApi)
    builder.Services.AddBaseControllerInterceptors<ImportSettingsApiController>();

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true; 
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; 
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Converters.Add(new EmptyGuidConverter());
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("shopSettingType", typeof(EnumRouteConstraint<ShopSettingType>)); 
});


AddLogging(builder.Configuration, builder.Logging, isApi);

var app = builder.Build();

if (!isApi)
    app.MapReverseProxy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else if (isApi)
    app.UseBaseInterceptors<ImportSettingsApiController>();

if (isApi)
    app.UseApiSwagger();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

if (isApi)
    app.SetApiRoute("Hello ImportSettingsWebApp API!");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void AddLogging(IConfiguration configuration, ILoggingBuilder loggingBuilder, bool isApi)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextFile = "log.property.json";
    var serviceName = "Alchemist.Product.ImportSettingsWebApp";
    var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);

    loggerConfiguration.AddServiceBaseConfigs(logContextFile, logPath, serviceName);
    if (isApi)
    {
        loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
        loggerConfiguration.AddSourceContextConfig(logContextFile, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());
    }
    loggerConfiguration.SetSerilog(loggingBuilder);
}

public class ImportSettingsWebAppProgramm
{ }

