namespace Alchemist.Product.Import.WebApp.Models;

public class ImportViewModel(List<string> shops, string? selectedShop, string selectedTabName)
{
    public ImportViewModel(List<string> shops) : this(shops, shops.FirstOrDefault(), "Settings")
    {
    }

    public List<string> Shops { get; } = shops;

    public string? SelectedShop { get; } = selectedShop;

    public string SelectedTabName { get; } = selectedTabName;
}
