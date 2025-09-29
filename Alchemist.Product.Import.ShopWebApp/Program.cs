using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Log.Extensions;
using Alchemist.Product.RestAPIClient;
using Alchemist.Product.ShopWebApp.Controllers;
using Http.ErrorHandling;
using Http.Info;
using Serilog.Configuration.Extensions;



var builder = WebApplication.CreateBuilder(args);

var isApi = args.Length > 0 && 
    args.Contains("-api", StringComparer.InvariantCultureIgnoreCase)
    || args.Contains("--api=true", StringComparer.InvariantCultureIgnoreCase)
    || builder.Configuration.GetValue<bool>("isApi");

builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "ShopAPIHost", nameof(ShopApiClient), out IHttpClientBuilder shopHttpClientBuilder);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ShopApp.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.IsEssential = true;
});

if(isApi)
{
    builder.Services.AddAuthentication("https");
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

if(isApi)
{
    builder.Services.AddExceptionHandler<GlobalExceptionHandler<ShopApiController>>();
    builder.Services.AddSingleton<InfoLogMiddleware<ShopApiController>>();
    builder.Services.AddProblemDetails();
}

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shop/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else if(isApi)
{
    app.UseExceptionHandler();
    app.UseMiddleware<InfoLogMiddleware<ShopApiController>>();
}

if(isApi)
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });
}

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

app.UseSession();

if(isApi)
{
    app.UseHsts();

    app.MapControllers();

    app.MapGet("/", () => "Hello ShopWebApp API!");
}
else
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();

public class ShopWebAppProgram
{ }
