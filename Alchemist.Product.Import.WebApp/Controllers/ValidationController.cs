using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ValidationController : Controller
{
    [AcceptVerbs("GET", "POST")]
    public IActionResult AssemblyPathOrProviderPathNotEmpty(string serviceTypeName, string assemblyPath, string serviceProviderPath)
    {
        return string.IsNullOrEmpty(assemblyPath) && string.IsNullOrEmpty(serviceProviderPath) ? Json(false) : (IActionResult)Json(true);
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult AssemblyPathForJsonValueNotEmpty(string jsonValue, string assemblyPath)
    {
        return string.IsNullOrEmpty(assemblyPath)  ? Json(false) : (IActionResult)Json(true);
    }
}
