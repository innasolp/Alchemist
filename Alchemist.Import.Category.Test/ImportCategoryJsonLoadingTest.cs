using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Category.Service.Json;
using Alchemist.Import.CategoryService.Test.Infrastructure;
using Import.Interfaces;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using System;
using System.Resources;
using Xunit.Abstractions;

namespace Alchemist.Import.CategoryService.Test;

public class ImportCategoryJsonLoadingTest : ImportServiceTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesJsonTimerService>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryLoadOptions _loadOptions = new() { SecondsInterval = 30 };

    private readonly Mock<ICategoryItemHandler> _categoryItemHandlerMock = new();

    protected ResourceManager ImportCategoriesResourceManager { get; }

    public ImportCategoryJsonLoadingTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ImportCategoriesResourceManager = new ResourceManager("Alchemist.Import.Category.Service.ImportCategoryLogMessages",
                               typeof(ShopImportCategoriesTimerService<>).Assembly);

        LoaderMock.SetupLoadCookies();
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.Name).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.SourceName).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.SourceUrl).Returns(Guid.NewGuid().ToString());
    }

    protected override ShopImportCategoriesTimerServiceTest CreateService(string name)
    {
        return new ShopImportCategoriesTimerServiceTest(LoggerMock.Object,
            name,
            null,
            LoaderMock.Object,
            _categoryShopModelMock.Object,
            _loadOptions,
            _categoryItemHandlerMock.Object
            );
    }

    private async Task ExecuteServiceAsync(string name, int executionDuration)
    {
        var service = CreateService(name);

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(executionDuration);

        await service.Start(tokenSource.Token); 
    }

    [Fact]
    public async Task LogInfoSuccessWhenCategoriesLoaded()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var category = Helper.CreateCategoryWithChildren();
        _loadOptions.CategoryPropertyPaths = new Dictionary<string, PropertyPath>() { { "Url", new PropertyPath("Url", "Url") },
            { "Description",new PropertyPath("Description", "Description") },
            { "Children",new PropertyPath("Children", "Children") },
            { "Name",new PropertyPath("Name", "Name") },
            { "Id",new PropertyPath("Id", "Id") } };

        var requestData = new { id = 2 };
        var loadAutoResetEvent = new AsyncAutoResetEvent();
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w =>
           w.Load(_categoryShopModelMock.Object.CategorySourceUrl, It.Is<object[]>(data => data.Contains(requestData)), It.IsAny<CancellationToken>()))
           .Returns(async (string url, object? data, CancellationToken cancellationToken) =>
           {
               var stream = await TestExtensions.LoadItemAsync(category);
               loadAutoResetEvent.Set();
               return stream;
           });        
        
        var name = Guid.NewGuid().ToString();

        await ExecuteServiceAsync(name, 500);

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoryNameIdForShopWasLoaded"),
            category.Name, category.Id, _categoryShopModelMock.Object.SourceName); 
    }

    [Fact]
    public async Task LogErrorWhenJsonLoadFromCategorySourceUrlFailed()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();       

        var exception = new LoaderServiceException("Json loading failed");
        var requestData = new {id = 10};
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w=>
            w.Load(_categoryShopModelMock.Object.CategorySourceUrl, It.Is<object[]>(data=>data.Contains(requestData)), It.IsAny<CancellationToken>()))
            .Throws(exception);

        var name = Guid.NewGuid().ToString();

        await ExecuteServiceAsync(name, 500);

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoriesWereNotLoaded"),
            _categoryShopModelMock.Object.SourceName, _categoryShopModelMock.Object.SourceUrl);
    }
}