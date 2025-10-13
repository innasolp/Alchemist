using Alchemist.Product.ImportSettingsWebApp.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Alchgemist.Product.ImportSettingsWebApp.UnitTests.Controllers;

public class ValidationControllerTests
{
    [Fact()]
    public void AssemblyPathOrProviderPathNotEmptyWhenOnlyServiceTypeNameIsNotEmptyTest()
    {
        var controller = new ValidationController();

        // both empty -> false
        var resultEmpty = controller.AssemblyPathOrProviderPathNotEmpty("Service", string.Empty, string.Empty) as JsonResult;
        Assert.NotNull(resultEmpty);
        Assert.Equal(false, resultEmpty.Value);        
    }

    [Fact()]
    public void AssemblyPathOrProviderPathNotEmptyReturnsFalseWhenOnlyServiceProviderPathIsEmptyTest()
    {
        var controller = new ValidationController();

        // assemblyPath provided -> true
        var resultAssembly = controller.AssemblyPathOrProviderPathNotEmpty("Service", "some.dll", string.Empty) as JsonResult;
        Assert.NotNull(resultAssembly);
        Assert.Equal(true, resultAssembly.Value);
    }

    [Fact()]
    public void AssemblyPathOrProviderPathNotEmptyReturnsFalseWhenOnlyAssemblyPathIsEmptyTest()
    {
        var controller = new ValidationController();

        // serviceProviderPath provided -> true
        var resultProvider = controller.AssemblyPathOrProviderPathNotEmpty("Service", string.Empty, "provider") as JsonResult;
        Assert.NotNull(resultProvider);
        Assert.Equal(true, resultProvider.Value);
    }

    [Fact()]
    public void AssemblyPathForJsonValueReturnsFalseWhenAssemblyPathIsEmptyTest()
    {
        var controller = new ValidationController();

        // assemblyPath empty -> false
        var resultEmpty = controller.AssemblyPathForJsonValueNotEmpty("{\"a\":1}", string.Empty) as Microsoft.AspNetCore.Mvc.JsonResult;
        Assert.NotNull(resultEmpty);
        Assert.Equal(false, resultEmpty.Value);

        
    }

    [Fact()]
    public void AssemblyPathForJsonValueReturnsTrueWhenAssemblyPathNotEmptyTest()
    {
        var controller = new ValidationController();

        // assemblyPath provided -> true
        var resultAssembly = controller.AssemblyPathForJsonValueNotEmpty("{\"a\":1}", "some.dll") as JsonResult;
        Assert.NotNull(resultAssembly);
        Assert.Equal(true, resultAssembly.Value);
    }

    [Fact()]
    public void InvalidJsonValueReturnsTrueWhenJsonIsEmptyTest()
    {
        var controller = new ValidationController();

        // null or empty -> true
        var resultEmpty = controller.InvalidJsonValue(string.Empty) as JsonResult;
        Assert.NotNull(resultEmpty);
        Assert.Equal(true, resultEmpty.Value);
    }

    [Fact()]
    public void InvalidJsonValueReturnsTrueWhenJsonIsValidTest()
    {
        var controller = new ValidationController();
        // valid json -> true
        var validJson = "{\"key\":\"value\"}";
        var resultValid = controller.InvalidJsonValue(validJson) as JsonResult;
        Assert.NotNull(resultValid);
        Assert.Equal(true, resultValid.Value);
    }

    [Fact()]
    public void InvalidJsonValueReturnsTrueWhenJsonIsInvalidTest()
    {
        var controller = new ValidationController();

        // invalid json -> false
        var invalidJson = "{key: value"; // malformed
        var resultInvalid = controller.InvalidJsonValue(invalidJson) as JsonResult;
        Assert.NotNull(resultInvalid);
        Assert.Equal(false, resultInvalid.Value);
    }
}