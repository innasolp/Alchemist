using Alchemist.Product.ImportSettingsWebApp.Controllers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class UploadFromFileControllerTests
{
    [Fact]
    public async Task UploadFromFileAsync_NullFile_ReturnsBadRequest()
    {
        var controller = new UploadFromFileController();

        var result = await controller.UploadFromFileAsync(null) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal("file", result.Value);
    }

    [Fact]
    public async Task UploadFromFileAsync_ValidJsonFile_ReturnsOkWithJsonString()
    {
        var controller = new UploadFromFileController();

        var json = "{\"key\":\"value\"}";
        var bytes = Encoding.UTF8.GetBytes(json);
        await using var stream = new MemoryStream(bytes);

        // Reset position for FormFile creation
        stream.Position = 0;
        IFormFile formFile = new FormFile(stream, 0, stream.Length, "file", "test.json")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/json"
        };

        var actionResult = await controller.UploadFromFileAsync(formFile) as OkObjectResult;

        Assert.NotNull(actionResult);
        Assert.IsType<string>(actionResult.Value);

        var returnedJson = actionResult.Value as string;
        // The controller deserializes and then re-serializes using JsonNode.ToJsonString(),
        // which should produce an equivalent JSON string. Compare by parsing both to JsonNode.
        var originalNode = System.Text.Json.Nodes.JsonNode.Parse(json);
        var returnedNode = System.Text.Json.Nodes.JsonNode.Parse(returnedJson);

        Assert.NotNull(originalNode);
        Assert.NotNull(returnedNode);
        Assert.Equal(originalNode.ToJsonString(), returnedNode.ToJsonString());
    }
}