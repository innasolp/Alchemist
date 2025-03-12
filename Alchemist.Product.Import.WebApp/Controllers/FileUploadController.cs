using Alchemist.Product.Import.Model;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class FileUploadController(IImportFacade importFacade) : Controller
{
    private readonly IImportFacade _importFacade = importFacade;
    
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
    public async Task<ShopSettingsModel?> UploadShopSettings(Guid shopGuid, int shopSettingsType, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        using var stream = file.OpenReadStream();

        var uploadedShopSettings = await stream.GetShopSettingsFromJsonAsync((ShopSettingType)shopSettingsType);

        if (uploadedShopSettings != null && _importFacade.TryGetShopSettings(shopGuid, (ShopSettingType)shopSettingsType, out var shopSettings))         
        {
            shopSettings?.Update(uploadedShopSettings, true);
            shopSettings.FileName = file.FileName;

            return shopSettings;
        }

        return null;
    }

    [HttpPost]
    public async Task<ServiceSettingsModel?> UploadServiceSettings(Guid shopGuid, Guid  shopSettingsGuid, string serviceSettingsName, IFormFile file)
    {
        //todo for net.9
        //JsonSerializerOptions options = new()
        //{
        //    RespectRequiredConstructorParameters = true
        //};

        var uplodedServiceSettings = await GetFromJsonAsync<ServiceSettingsModel>(file);
        
        if (uplodedServiceSettings != null && _importFacade.TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
        {
            uplodedServiceSettings.Name = serviceSettingsName;
            uplodedServiceSettings.FileName = file.FileName;            

            shopSettings?.UpdateServiceSettings(uplodedServiceSettings);

            return uplodedServiceSettings;
        }

        return null;
    }
}
