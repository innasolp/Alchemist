using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class Product : IProduct
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string? Transcript { get; set; }

    public int? BrandId { get; set; }

    public string? Articul { get; set; }

    public int? InitShopId { get; set; }

    public DateTime AddedTime { get; set; }

    public short ProductTypeId { get; set; }
}
