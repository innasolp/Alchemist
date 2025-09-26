using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Product.RestAPIClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "ShopAPIHost", nameof(ShopApiClient), out IHttpClientBuilder shopHttpClientBuilder);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ShopApp.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shop/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");

app.Run();
