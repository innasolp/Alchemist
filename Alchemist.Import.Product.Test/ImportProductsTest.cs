using Alchemist.Import.Product.Test.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Test.Import.Service;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.ObjectModel;
using System.Resources;

using Xunit.Abstractions;

namespace Alchemist.Import.Product.Test;

public abstract class ImportProductsTest : ImportServiceTest<TestImportProductService<TestCategory, TestProductItem>, ILogger>
{
    protected ResourceManager ImportProductsResourceManager { get; }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new Mock<IProductShopModel>();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

    protected override TestImportProductService<TestCategory, TestProductItem> Service { get; }
        

    protected ImportProductsTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ImportProductsResourceManager = new ResourceManager("Alchemist.Import.Products.Service.ImportProductLogMessages",
                               typeof(ShopImportCategoryProductsService<TestCategory, TestProductItem>).Assembly);
       
        ProductShopModelMock.Setup(s => s.Categories).Returns(new ObservableCollection<IProductShopCategory>());        

        Service = new TestImportProductService<TestCategory, TestProductItem>( 
            LoggerMock.Object,
             ProductShopModelMock.Object,
             WebLoaderMock.Object,
             BrowserServiceMock.Object,
             RequestHeaders,
             ProductItemHandlerMock.Object);        
    }
}
