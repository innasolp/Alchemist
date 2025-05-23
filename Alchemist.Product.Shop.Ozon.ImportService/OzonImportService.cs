using Alchemist.Product.Shop.Ozon.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using WebLoader.Common;
namespace Alchemist.Product.Shop.Ozon.ImportService;

public class OzonImportService(ILogger<OzonImportService> logger,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IProductShopModel shopUrlModel,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IWebLoader shopImporter,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] RequestHeaders requestHeaders,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IProductItemHandler itemHandler
        ) : ShopImportCategoryProductsService<Category, Model.Product>(logger, shopUrlModel, shopImporter, requestHeaders, itemHandler)
{
    public override string Name => "OzonImport";

    protected override int PageProductCount => 12;

    protected override string GetApiUrl(ICategoryProductItem productItem)
    {
        var ozonCategoryItem = productItem as ProductItem;
        return ozonCategoryItem != null
            ? string.Format(ProductShopModel.ProductUrl, ozonCategoryItem.Name)
            : throw new InvalidCastException("Item is not Ozon");
    }
    
    protected override bool IsEndOfCategory(Category category)
    {
        return category.CategoryContent == null;
    }
}
