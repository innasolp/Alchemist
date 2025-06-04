using Alchemist.Product.Import.Model;
using Alchemist.Import.Settings.Interfaces;
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
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadShopSettings(Guid shopGuid, int shopSettingsType, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(nameof(file));

        if (shopGuid == Guid.Empty)
            return BadRequest(nameof(shopGuid));

        //using var stream = file.OpenReadStream();
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var uploadedShopSettings = await stream.GetShopSettingsFromJsonAsync((ShopSettingType)shopSettingsType);
        if (uploadedShopSettings == null)
            return BadRequest(shopSettingsType);

        if (_importFacade.TryGetShopSettings(shopGuid, (ShopSettingType)shopSettingsType, out var shopSettings))         
        {
            shopSettings?.Update(uploadedShopSettings, true);
            shopSettings.FileName = file.FileName;

            return Ok(shopSettings);
        }

        return NotFound(shopGuid);
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadServiceSettings(Guid shopGuid, Guid  shopSettingsGuid, string serviceSettingsName, IFormFile file)
    {
        //todo for net.9
        //JsonSerializerOptions options = new()
        //{
        //    RespectRequiredConstructorParameters = true
        //};
        if (file == null || file.Length == 0)
            return BadRequest(nameof(file));
        
        if (shopGuid == Guid.Empty)
            return BadRequest(nameof(shopGuid));
        
        if (shopSettingsGuid == Guid.Empty)
            return BadRequest(nameof(shopSettingsGuid));

        if (string.IsNullOrEmpty(serviceSettingsName))
            return BadRequest(nameof(serviceSettingsName)); 

        var uplodedServiceSettings = await GetFromJsonAsync<ServiceSettingsModel>(file);
        if (uplodedServiceSettings == null)
            return BadRequest(nameof(file));

        if (_importFacade.TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
        {
            uplodedServiceSettings.Name = serviceSettingsName;
            uplodedServiceSettings.FileName = file.FileName;

            if (shopSettings.GetServiceSettings(uplodedServiceSettings.Name) == null)
                shopSettings.SetServiceSettings(shopSettings.CreateServiceSettingsModel(uplodedServiceSettings.Name));


            shopSettings?.UpdateServiceSettings(uplodedServiceSettings);

            return Ok(uplodedServiceSettings);
        }

        return NotFound(shopGuid);
    }
}
