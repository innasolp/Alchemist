using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.RestAPIClient;
using Alchemist.Product.ShopWebApp.Controllers;
using Http.ErrorHandling;
using Http.Info;
using Serilog.Configuration.Extensions;
using Alchemist.WebApp.Api.Common;

var builder = WebApplication.CreateBuilder(args);

bool isApi = builder.IsApi(args);

builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "ShopAPIHost", nameof(ShopApiClient), out IHttpClientBuilder shopHttpClientBuilder);

// Add services to the container.
builder.Services.AddControllersWithViews();


if (isApi)
    builder.Services.AddSwaggerApi();

if (isApi)
    builder.Services.AddBaseControllerInterceptors<ShopApiController>();

AddLogging(builder, isApi);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shop/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else if (isApi)
    app.UseBaseInterceptors<ShopApiController>();

if (isApi)
    app.UseApiSwagger();

app.UseHttpsRedirection();

// подключаем файлы по умолчанию
app.UseDefaultFiles();
// подключаем статические файлы
app.UseStaticFiles();

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
else
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();

static void AddLogging(WebApplicationBuilder builder, bool isApi)
{
    var logPath = $"{Utils.GetAppPath()}/Logs";
    var logContextPath = $"{builder.Environment.ContentRootPath}/log.property.json";
    var appSerilogBuilder = new SerilogConfigurationBuilder(builder.Configuration);
    var serviceName = "Alchemist.Product.ShopWebApp";
    appSerilogBuilder.AddServiceBaseConfigs(logContextPath, logPath, serviceName);
    if (isApi)
    {
        appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(InfoLogMiddleware<>).GetNameWithoutGenericArity());
        appSerilogBuilder.AddSourceContextContainsLogConfig(logContextPath, $"{logPath}/{serviceName}", typeof(GlobalExceptionHandler<>).GetNameWithoutGenericArity());
    }
    appSerilogBuilder.SetSerilog(builder.Logging);
}

public class ShopWebAppProgram
{ }
