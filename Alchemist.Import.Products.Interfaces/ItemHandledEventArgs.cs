namespace Alchemist.Import.Products.Interfaces;

public class ItemHandledEventArgs(IProductItem item, string apiUrl, bool success) : EventArgs
{
    public bool Success { get; } = success;

    public IProductItem Item { get; } = item;

    public string ApiUrl { get; } = apiUrl;
}