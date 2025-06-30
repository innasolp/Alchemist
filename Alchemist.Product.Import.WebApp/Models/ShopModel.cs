using Alchemist.Product.Import.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ShopModel : IShopModel
{
    public ShopModel(int id)
    {
        Id = id;
        Guid = Guid.NewGuid();
    }

    [JsonConstructor]
    public ShopModel() : this(0){}
    
    public Guid Guid { get; set; }

    [Required]
    public string Name { get; set; }

    public int Id { get; init; }

    public bool IsDeprecated { get; set; } = false;

    [Required]
    public string Url { get; set; }

    public string? Caption { get; set; }    
}
