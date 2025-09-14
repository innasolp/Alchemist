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
            LoaderMock.Object,
            _categoryShopModelMock.Object,            
            _loadOptions,
            _categoryItemHandlerMock.Object
            );
    }

    [Fact]
    public async Task StoppedWhenWebLoaderNotExecutedAsync()
    {
        LoaderMock.Reset();
        await ImportWasStoppedWhenLoaderNotExecutedAsync();
    }

    [Fact]
    public async Task StartedWhenWebLoaderExecutedSuccessfullAsync()
    {
        LoaderMock.Reset();
        await ImportStartedWhenLoaderExecutedSuccessfullAsync();
    }

    [Fact]
    public async Task StoppedWhenCancellationRequestedAsync()
    {
        LoaderMock.Reset();

        var category = Helper.CreateCategoryWithChildren();
        var requestData = new object();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);
        _loadOptions.CategoryPropertyPaths = new Dictionary<string, PropertyPath>() { { "Url", new PropertyPath("Url", "Url") },
            { "Description",new PropertyPath("Description", "Description") },
            { "Children",new PropertyPath("Children", "Children") }, 
            { "Id",new PropertyPath("Id", "Id") } };

        await ImportStoppedWhenCancellationRequestedAsync();
    }

    [Fact]
    public async Task ImportFailedWhenLoaderAlwaysNeedReseting()
    {
        LoaderMock.Reset();       

        await ImportFailedWhenLoaderAlwaysNeedResetingAsync();
    }
}