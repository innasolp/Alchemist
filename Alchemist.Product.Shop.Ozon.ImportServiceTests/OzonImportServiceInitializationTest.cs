using Microsoft.Extensions.Logging;
using Alchemist.Product.Shop.Ozon.ImportService;
using Xunit.Abstractions;
using WebLoader.Interfaces;
using System.Reflection;
using System.Text.Json;
using Alchemist.Import.Products.Interfaces;
using WebLoader.Common;

namespace Alchemist.Product.Shop.Ozon.ImportServiceTests;

public class OzonImportServiceInitializationTest
{
    private readonly ILogger<OzonImportService> _logger = Moq.Mock.Of<ILogger<OzonImportService>>();
    private readonly Moq.Mock<IProductShopModel> _shopUrlModelMock = new();
    private readonly IWebLoader _webLoader = Moq.Mock.Of<IWebLoader>();
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly string _requestHeadersFireFoxFileName = "Ozon.Headers.Firefox.json";
    private readonly string _requestHeadersChromeFileName = "Ozon.Headers.Chrome.json";

    private readonly Moq.Mock<IProductItemHandler> _productItemHandler = new();

    public OzonImportServiceInitializationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _shopUrlModelMock.Setup(m => m.Categories).Returns(new System.Collections.ObjectModel.ObservableCollection<IProductShopCategoryModel>());
    }

    [Fact]
    public void RequestChromeHeadersLoadTest()
    {
        RequestHeadersLoadTest(_requestHeadersChromeFileName);
    } 
    
    [Fact]
    public void RequestFirefoxHeadersLoadTest()
    {
        RequestHeadersLoadTest(_requestHeadersFireFoxFileName);
    }

    private void RequestHeadersLoadTest(string requestHeadersFileName)
    {
        OzonImportService? ozonImportService = null;
        Exception? ex = null;

        using var s = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{requestHeadersFileName}");

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(s);

        s.Close();

        try
        {
            //todo
            ozonImportService = new OzonImportService(_logger,  _shopUrlModelMock.Object, _webLoader, requestHeaders, _productItemHandler.Object);
        }
        catch (Exception e) { ex = e; }

        if (ex != null) _testOutputHelper.WriteLine(ex.Message);

        Assert.NotNull(ozonImportService);
        Assert.NotNull(requestHeaders);
        Assert.NotEmpty(requestHeaders.Headers);
        Assert.NotEmpty(requestHeaders.CookieKeys);
    }
}