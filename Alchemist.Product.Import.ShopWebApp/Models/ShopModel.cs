using Shop.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.ShopWebApp.Models;

public class ShopModel : IShop
{
    [Required]
    public string Name { get; set; }

    public int Id { get; set; }

    [Required]
    public string Url { get; set; }

    public string? Caption { get; set; }
}
