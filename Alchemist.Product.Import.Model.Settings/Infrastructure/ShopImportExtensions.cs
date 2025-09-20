namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ShopImportExtensions
{
    public static ITabModel GetTab(this ShopImportModel shopImport, TabType tab)
    {
        return tab == TabType.Shop
             ? shopImport.ShopSettingTabs
             : tab == TabType.Products ? shopImport.ImportProducts : shopImport.ImportCategories;
    }    
}
