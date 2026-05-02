using Shop.Interfaces;

namespace Alchemist.Product.ShopWebApp.UnitTests.Infrastructure;

internal class Shop : IShop
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Url { get; set; }

    public string? Caption { get; set; }
}
