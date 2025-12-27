using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Import.ProductService.Test.Infrastructure;
using Import.Service.Test;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace Alchemist.Import.ProductService.Test;

public abstract class ImportProductsTest : ImportServiceTest<TestImportProductService<TestCategory, TestProductItem>, ILogger>
{
    private int _pageProductCount = 1000;

    protected ResourceManager ImportProductsResourceManager { get; }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new ();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();
    
    protected ImportProductsTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ImportProductsResourceManager = new ResourceManager("Alchemist.Import.Products.Service.ImportProductLogMessages",
                               typeof(ShopImportCategoryProductsService<TestCategory, TestProductItem>).Assembly);
       
        ProductShopModelMock.Setup(s => s.Categories).Returns([]); 
    }

    protected void SetPageProductCount(int pageProductCount)=> _pageProductCount = pageProductCount;

    protected override TestImportProductService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
            LoggerMock.Object,
            name,
             ProductShopModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             pageProductCount : _pageProductCount);
    }
}