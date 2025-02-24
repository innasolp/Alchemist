namespace Alchemist.Product.Import.WebApp.Infrastructure;

public static class ViewHelper
{
    public static string GetMenuItemStyle<T>(T item, T selectedItem)        
    {
        return "menu_item" + (selectedItem.Equals(item) ? " selected" : "");
    }
    
    public static string GetAnchorMenuItemStyle<T>(T item, T selectedItem)        
    {
        return "menu_item-a" + (selectedItem.Equals(item) ? " selected-a" : "");
    }

    public static string GetLiStyle(string item, string selectedItem) => "left-menu-ul" + (selectedItem == item ? " selected" : "");
}
