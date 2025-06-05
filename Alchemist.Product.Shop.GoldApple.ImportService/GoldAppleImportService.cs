using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Alchemist.Product.Shop.GoldApple.Model;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using WebLoader.Common;

namespace Alchemist.Product.Shop.GoldApple.ImportService;

public class GoldAppleImportService(ILogger<GoldAppleImportService> logger,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IProductShopModel shopUrlModel,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IWebLoader shopImporter,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] RequestHeaders requestHeaders,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IProductItemHandler productDataHandler)
    : ShopImportCategoryProductsService<CategoryProducts, ProductData>(logger, shopUrlModel, shopImporter, requestHeaders, productDataHandler)
{
    public override string Name => "GoldAppleImport";

    protected override int PageProductCount => 24;

    protected override bool IsEndOfCategory(CategoryProducts category)
    {
        return !(category.Data.Products?.Length > 0) ;
    }

    protected override string GetApiUrl(ICategoryProductItem productItem)
    {
        return string.Format(ProductShopModel.ProductUrl, productItem.Id);
    }
}
