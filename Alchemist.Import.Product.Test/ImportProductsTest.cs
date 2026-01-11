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
    protected ResourceManager ImportProductsResourceManager { get; }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new ();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();
    
    protected ImportProductsTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ImportProductsResourceManager = new ResourceManager("Alchemist.Import.Products.Service.ImportProductLogMessages",
                               typeof(ShopImportCategoryProductsService<TestCategory, TestProductItem>).Assembly);
       
        ProductShopModelMock.Setup(s => s.Categories).Returns([]); 
    }
    
    protected override TestImportProductService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
            LoggerMock.Object,
            name,
             ProductShopModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             1000);
    }

    protected TestImportProductService<TestCategory, TestProductItem> CreateService(string name, int pageProductCount)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
            LoggerMock.Object,
            name,
             ProductShopModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             pageProductCount);
    }
}