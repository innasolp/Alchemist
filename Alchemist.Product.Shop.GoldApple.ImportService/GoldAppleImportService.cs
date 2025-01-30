using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Alchemist.Product.Shop.GoldApple.Model;
using WebLoader.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace Alchemist.Product.Shop.GoldApple.ImportService;

public class GoldAppleImportService(ILogger<GoldAppleImportService> logger,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IShopUrlModel shopUrlModel,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IWebLoader shopImporter)
    : ShopImportCategoryProductsService<CategoryProducts, ProductData>(logger, shopUrlModel, shopImporter, null)
{
    public override string Name => "GoldAppleImport";

    protected override bool IsEndOfCategory(CategoryProducts category)
    {
        return category.Data.Products == null;
    }

    protected override string GetApiUrl(ICategoryProductItem productItem)
    {
        return string.Format(ShopUrlModel.ProductUrl, productItem.Id);
    }
}
