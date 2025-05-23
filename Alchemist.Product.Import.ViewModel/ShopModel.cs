using Alchemist.Product.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.Import.Model;

public class ShopModel : IShop
{
    public Guid Guid { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; }

    public int Id { get; set; }

    public bool IsDeprecated { get; set; } = false;

    [Required]
    public string ShopUrl { get; set; }

    public string? Caption { get; set; }
    string IShop.Url { get => ShopUrl; set => ShopUrl = value; }

    public void Update(IShop shop)
    {
        Name = shop.Name;
        ShopUrl = shop.Url;
        Caption = shop.Caption;
    }
}
