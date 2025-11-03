namespace Alchemist.Product.WebApp.Models;

public enum Tab
{
    Shops = 0,
    ImportSettings  = 1
}

public class IndexModel
{
    public Tab Tab { get; set; }
}
