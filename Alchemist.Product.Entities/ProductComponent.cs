
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ProductComponent:IProductComponent
{
    public long ProductId { get; set; }
    public int ComponentId { get; set; }
    public short SequalNumber { get; set; }
}
