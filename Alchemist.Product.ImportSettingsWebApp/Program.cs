using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchemist.Settings.RestAPIClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out IHttpClientBuilder settingsHttpClientBuilder);
builder.Services.AddKeyedTypedSettingsDataAdapter<ProductShopImportSettingsModel, ServiceSettingsModel>(ShopSettingType.Product, ShopSettingType.Product);
builder.Services.AddKeyedTypedSettingsDataAdapter<CategoryShopImportSettingsModel, ServiceSettingsModel>(ShopSettingType.Category, ShopSettingType.Category);


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".ShopImportSettingsApp.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.IsEssential = true;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapReverseProxy();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
