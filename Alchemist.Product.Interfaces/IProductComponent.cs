namespace Alchemist.Product.Interfaces;

public interface IProductComponent
{
    public long ProductId { get; set; }
    public int ComponentId { get; set; }
    public short SequalNumber { get; set; }
}
