using Shop.Interfaces;

namespace Shop.API.Client;

internal class Shop:IShop
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Url { get; set; }

    public string? Caption { get; set; }
}