namespace Alchemist.Product.ShopWebApp.Models;

public class IndexModel
{
    public bool ShopsUploaded { get; set; } = false;

    internal ShopTabModel ShopTab { get; set; }
}
