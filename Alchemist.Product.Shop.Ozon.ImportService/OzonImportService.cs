using Alchemist.Product.Shop.Ozon.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebLoader.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace Alchemist.Product.Shop.Ozon.ImportService;

public class OzonImportService(ILogger<OzonImportService> logger,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IShopUrlModel shopUrlModel,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IWebLoader shopImporter,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] RequestHeaders requestHeaders
        ) : ShopImportCategoryProductsService<Category, Model.Product>(logger, shopUrlModel, shopImporter, requestHeaders)
{
    public override string Name => "OzonImport";
    
    protected override string GetApiUrl(ICategoryProductItem productItem)
    {
        var ozonCategoryItem = productItem as ProductItem;
        return ozonCategoryItem != null
            ? string.Format(ShopUrlModel.ProductUrl, ozonCategoryItem.Name)
            : throw new InvalidCastException("Item is not Ozon");
    }
    
    protected override bool IsEndOfCategory(Category category)
    {
        return category.CategoryContent == null;
    }
}
