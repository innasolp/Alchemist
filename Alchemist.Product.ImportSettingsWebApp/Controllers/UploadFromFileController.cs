using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class UploadFromFileController : Controller
{
    [Route("Upload/Json")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]    
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFromFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(nameof(file));        

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var jsonobject = await JsonSerializer.DeserializeAsync<JsonObject>(stream, cancellationToken: cancellationToken);

        return Ok(jsonobject.ToJsonString());
    }
}
