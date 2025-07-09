namespace Alchemist.Product.Import.Model;

public interface IShopModel : IModel
{
    Guid Guid { get; }

    string Name { get; set; }

    int Id { get; }

    bool IsDeprecated { get; set; }

    string Url { get; set; }

    string? Caption { get; set; }
}
