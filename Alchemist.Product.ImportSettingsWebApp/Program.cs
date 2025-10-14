using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchemist.Settings.RestAPIClient;
using Http.ErrorHandling;
using Http.Info;
using Serilog.Configuration.Extensions;
using Alchemist.Log.Extensions;
using Alchemist.WebApp.Api.Common;

var builder = WebApplication.CreateBuilder(args);

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
    builder.Services.AddSwaggerApi();

if (isApi)
    builder.Services.AddBaseControllerInterceptors<ImportSettingsApiController>();

// Add services to the container.
builder.Services.AddControllersWithViews();

AddLogging(builder, isApi);

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
else
    app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void AddLogging(WebApplicationBuilder builder, bool isApi)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
    var appSerilogBuilder = new SerilogConfigurationBuilder(builder.Configuration);
    var serviceName = "Alchemist.Product.ImportSettingsWebApp";
    appSerilogBuilder.AddServiceBaseConfigs(logContextPath, logPath, serviceName);
    if (isApi)
    {
        appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
        appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());
    }
    appSerilogBuilder.SetSerilog(builder.Logging);
}

public class ImportSettingsWebAppProgramm
{ }

