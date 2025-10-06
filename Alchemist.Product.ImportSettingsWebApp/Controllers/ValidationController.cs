using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

public class ValidationController : Controller
{
    [AcceptVerbs("GET", "POST")]
    public IActionResult AssemblyPathOrProviderPathNotEmpty(string serviceTypeName, string assemblyPath, string serviceProviderPath)
    {
        return string.IsNullOrEmpty(assemblyPath) && string.IsNullOrEmpty(serviceProviderPath) ? Json(false) : Json(true);
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult AssemblyPathForJsonValueNotEmpty(string jsonValue, string assemblyPath)
    {
        return string.IsNullOrEmpty(assemblyPath)  ? Json(false) : Json(true);
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult InvalidJsonValue(string stringValue)
    {
        if(string.IsNullOrEmpty(stringValue)) return Json(true);
            
        try
        {
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(stringValue);
            return Json(true);
        }
        catch
        {
            return Json(false);
        }
    }
}
