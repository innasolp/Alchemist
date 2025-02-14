using Alchemist.Product.Import.WebApp.Models;

namespace Alchemist.Product.Import.WebApp.Infrastructure;

public static class TabFactory
{
    public static ITabViewModel CreateTabViewModel(string? tabViewName, ShopModel shop)
    {
        switch(tabViewName)
        {
            case "Settings":
                return new SettingsViewModel(shop);

            case "ImportProducts":
                return new ImportProductsViewModel(shop);

            case "ImportCategories":
                return new ImportCategoriesViewModel(shop);

            default:
                return new SettingsViewModel(shop);
        }
    }   
}
