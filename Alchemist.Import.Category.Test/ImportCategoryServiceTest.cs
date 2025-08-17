using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Alchemist.Import.Category.Test.Infrastructure;
using Alchemist.Import.Html;
using Alchemist.Test.Import.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using Alchemist.Test.Import.Service.Infrastructure; 

namespace Alchemist.Import.Category.Test;

public class ImportCategoryServiceTest : ImportServiceExecutionTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesTimerService>>
{
    protected override ShopImportCategoriesTimerServiceTest Service { get; }

    private readonly Mock<IHtmlSearcher> _htmlSearcherMock = new();

    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryLoadOptions _loadOptions = new() { SecondsInterval = 5 };

    private readonly Mock<ICategoryItemHandler> _categoryItemHandlerMock = new();    

    public ImportCategoryServiceTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
        
        Service = new ShopImportCategoriesTimerServiceTest(LoggerMock.Object,
            null,
            WebLoaderMock.Object,
            BrowserDataLoaderMock.Object,
            _categoryShopModelMock.Object,
            RequestHeaders,
            _loadOptions,
            _categoryItemHandlerMock.Object
            );
    }

    [Fact]
    public async Task StoppedWhenWebLoaderNotExecutedAsync()
    {
        WebLoaderMock.Reset();
        await ImportWasStoppedWhenWebLoaderNotExecutedAsync();
    }

    [Fact]
    public async Task StartedWhenWebLoaderExecutedSuccessfullAsync()
    {
        WebLoaderMock.Reset();
        await ImportStartedWhenWebLoaderExecutedSuccessfullAsync();
    }

    [Fact]
    public async Task StoppedWhenCancellationRequestedAsync()
    {
        WebLoaderMock.Reset();

        var category = Helper.CreateCategoryWithChildren();
        WebLoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, RequestHeaders, Cookies, category);
        _loadOptions.CategoryPropertyPaths = new Dictionary<string, PropertyPath>() { { "Url", new PropertyPath("Url", "Url") },
            { "Description",new PropertyPath("Description", "Description") },
            { "Children",new PropertyPath("Children", "Children") }, 
            { "Id",new PropertyPath("Id", "Id") } };

        await ImportStoppedWhenCancellationRequestedAsync();
    }
}