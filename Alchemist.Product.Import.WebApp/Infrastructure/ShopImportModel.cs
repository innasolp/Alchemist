namespace Alchemist.Product.Import.WebApp.Infrastructure;

public class ShopImportModel
{
    public int ShopId { get; set; }

    public ShopSettingsModel? ShopSettings { get; set; }

    public CategoriesImportSettingsModel? ImportCategories { get; set; }

    public ProductsImportSettingsModel? ImportProducts { get; set; }
}
