using Alchemist.Import.Product.Test.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.ObjectModel;
using System.Resources;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Product.Test;

public abstract class ImportProductsTest
{
    protected ResourceManager ServiceResourceManager { get; }

    protected ResourceManager ImportProductsResourceManager { get; }
    
    protected Mock<ILogger> LoggerMock { get; } = new Mock<ILogger>();

    protected Mock<IWebLoader> WebLoaderMock { get; } = new Mock<IWebLoader>();

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new Mock<IProductShopModel>();

    protected RequestHeaders RequestHeaders { get; } = new RequestHeaders();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

    internal TestImportProductService<TestCategory, TestProductItem> Service { get; }
        

    protected ImportProductsTest()
    {
        ImportProductsResourceManager = new ResourceManager("Alchemist.Import.Products.Service.ImportProductLogMessages",
                               typeof(ShopImportCategoryProductsService<TestCategory, TestProductItem>).Assembly);

        ServiceResourceManager = new ResourceManager("Alchemist.Import.Service.LogMessages",
                               typeof(ShopImportService).Assembly);

        ProductShopModelMock.Setup(s => s.Categories).Returns(new ObservableCollection<IProductShopCategoryModel>());        

        Service = new TestImportProductService<TestCategory, TestProductItem>( 
            LoggerMock.Object,
             ProductShopModelMock.Object,
             WebLoaderMock.Object,
             RequestHeaders,
             ProductItemHandlerMock.Object);        
    }
}
