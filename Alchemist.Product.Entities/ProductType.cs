using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ProductType:IProductType
{
    public short Id { get; set; }

    public string Name { get; set; }
}
