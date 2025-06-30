using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using Message.SignalR.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRestApiClient<IShopDataService, ShopApiClient>(builder.Configuration, "ShopAPIHost", nameof(ShopApiClient), out IHttpClientBuilder shopHttpClientBuilder);
builder.Services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(builder.Configuration, "SettingsAPIHost", nameof(SettingsAPIClient), out IHttpClientBuilder settingsHttpClientBuilder);
builder.Services.AddSettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel>();
builder.Services.AddSingleton<IModelFactory, ModelFactory>();
builder.Services.AddSingleton<IImportFacade, ImportFacade>();

var signalRUrl = builder.Configuration.GetHostSectionValue("ShopMessageReceiver");
builder.Services.AddSignalRMessageReceiver(signalRUrl);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class ImportWebAppProgram
{ }
