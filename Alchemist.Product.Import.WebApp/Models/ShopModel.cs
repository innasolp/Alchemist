using Alchemist.Product.Import.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public class ShopModel(int id) : IShopModel
{
    [JsonInclude]
    public Guid Guid { get; private set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; }

    public int Id { get; set; } = id;

    public bool IsDeprecated { get; set; } = false;

    [Required]
    public string Url { get; set; }

    public string? Caption { get; set; }    
}
