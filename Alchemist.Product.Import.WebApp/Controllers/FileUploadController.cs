using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class FileUploadController : Controller
{
    public ActionResult FileUpload()
    {
        return View();
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
        else
        {
            return "";
        }
    }
}
