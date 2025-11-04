namespace Alchemist.Product.WebApp.Models;

public enum Tab
{
    Shop = 0,
    ImportSettings  = 1
}

public class IndexModel
{
    public Tab Tab { get; set; }

    public object Data { get; set; }
}
