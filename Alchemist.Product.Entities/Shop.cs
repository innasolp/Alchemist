using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class Shop:IShop
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Url { get; set; }
}
