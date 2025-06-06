using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ProductPurpose : IProductPurpose
{
    public long ProductId { get; set; }

    public short PurposeTypeId { get; set; }
}
