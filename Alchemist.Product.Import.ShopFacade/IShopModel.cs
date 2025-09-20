namespace Alchemist.Product.Import.Model.Shop;

public interface IShopModel : IModel
{
    string Name { get; set; }

    int Id { get; }

    bool IsDeprecated { get; set; }

    string Url { get; set; }

    string? Caption { get; set; }
}
