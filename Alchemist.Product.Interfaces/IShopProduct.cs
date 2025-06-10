namespace Alchemist.Product.Interfaces;

public interface IShopProduct
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ItemId { get; set; }

    public int ShopId { get; set; }
    
    public bool? IsActual { get; set; }

    public string ApiUrl { get; set; }

    public string ItemUrl { get; set; }
}
