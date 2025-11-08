using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.Interfaces;
using Alchemist.WebApp.Api.Common;

AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    //todo
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews().ConfigureApplicationPartManager(manager =>
{
    // Example: Remove a specific assembly from controller discovery
    // manager.ApplicationParts.RemoveAll(part => part.Name == "AssemblyNameToExclude");

    // Example: Replace the default ControllerFeatureProvider to filter controllers
    // based on custom logic (e.g., attributes, naming conventions)

    //manager.ApplicationParts.Re
    var controllerFeatureProvider = manager.FeatureProviders
        .Single(p => p.GetType() == typeof(Microsoft.AspNetCore.Mvc.Controllers.ControllerFeatureProvider));
    manager.FeatureProviders[manager.FeatureProviders.IndexOf(controllerFeatureProvider)] = new ApiControllerFeatureProvider([typeof(HomeController)]);

    //manager.ApplicationParts.FirstOrDefault(ap=>ap.)
});

//builder.Services.Configure<RouteOptions>(options =>
//{
//    options.ConstraintMap.Add("shopSettingType", typeof(EnumRouteConstraint<ShopSettingType>));
//});

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

app.MapReverseProxy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
