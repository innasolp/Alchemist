using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Service.Test;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace ShopImport.Product.Service.Test.Infrastructure;

public abstract class ImportProductsTest<TService> : ImportServiceTest<TService, ILogger>
    where TService : ShopImportCategoryProductsService<TestCategory, TestProductItem>
{
    protected ResourceManager ImportProductsResourceManager { get; }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

    protected ImportProductsTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ImportProductsResourceManager = new ResourceManager("Alchemist.Import.Products.Service.ImportProductLogMessages",
                               typeof(ShopImportCategoryProductsService<TestCategory, TestProductItem>).Assembly);

        ProductShopModelMock.Setup(s => s.Categories).Returns([]);
    }
}