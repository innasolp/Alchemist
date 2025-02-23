using Alchemist.Product.Import.WebApp.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class FileUploadController(IDictionary<int, ShopImportModel> shopImports) : Controller
{
    private readonly IDictionary<int, ShopImportModel> _shopImports = shopImports;

    public ActionResult FileUpload()
    {
        return View();
    }

    private static async Task<T?> GetFromJsonAsync<T>(IFormFile file)
        where T:class
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            return await JsonSerializer.DeserializeAsync<T>(stream);            
        }
        return await Task.FromResult(default(T));
    }


    [HttpPost]
    public async Task<string?> UploadJson(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var jsonObj = await JsonSerializer.DeserializeAsync<JsonObject>(stream);
            return jsonObj?.ToString();
        }
        return "";
    }

    [HttpPost]
    public async Task<ShopSettingsModel?> UploadShopSettings(int shopId, IFormFile file)
    {
        var shopSettings = await GetFromJsonAsync<ShopSettingsModel>(file);

        if (shopSettings != null && _shopImports.TryGetValue(shopId, out var shopImportModel)
           && shopImportModel != null && shopImportModel.ShopSettings != null)
        {
            shopImportModel.ShopSettings.Update(shopSettings);

            return shopImportModel.ShopSettings;
        }

        return null;
    }  


    [HttpPost]
    public async Task<ServiceSettingsModel?> UploadServiceSettings(int shopId, string serviceSettingsName, IFormFile file)
    {
        //todo for net.9
        //JsonSerializerOptions options = new()
        //{
        //    RespectRequiredConstructorParameters = true
        //};

        var serviceSettings = await GetFromJsonAsync<ServiceSettingsModel>(file);

        if (serviceSettings == null) return null;

        if (_shopImports.TryGetValue(shopId, out var shopImportModel)
            && shopImportModel != null && shopImportModel.ShopSettings != null)
        {
            serviceSettings.ServiceName = serviceSettingsName;
            shopImportModel.ShopSettings.UpdateServiceSettings(serviceSettings);

            return serviceSettings;
        }

        return null;
    }
}
