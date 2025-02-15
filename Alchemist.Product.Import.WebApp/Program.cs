using Alchemist.Product.Import.WebApp.Models;
using System.Collections.ObjectModel;

var builder = WebApplication.CreateBuilder(args);

var shops = new List<ShopModel> {
    new() {Name = "Ozon", Id=1 },
    new() {Name = "Goldapple",Id=2 },
    new() {Name = "Test", Id=3 }};

builder.Services.AddSingleton(shops);

builder.Services.AddSingleton<IReadOnlyCollection<string>>(new ReadOnlyCollection<string> ( ["Settings", "ImportProducts", "ImportCategories"] ));

builder.Services.AddSingleton<List<ShopSettingsModel>>([]);
builder.Services.AddSingleton<List<ProductsImportSettingsModel>>([]);
builder.Services.AddSingleton<List<CategoriesImportSettingsModel>>([]);

var shopImports = new Dictionary<int,ShopImportModel>();
shops.ForEach(s => shopImports.Add(s.Id, new ShopImportModel { ShopId = s.Id }));
builder.Services.AddSingleton<IDictionary<int, ShopImportModel>>(shopImports);


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
