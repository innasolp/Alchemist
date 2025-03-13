namespace Alchemist.Product.Import.Model;

public class ShopModel
{
    public Guid Guid { get; } = Guid.NewGuid();

    public required string Name { get; set; }

    public int Id { get; set; }

    public bool IsDeprecated { get; set; } = false;
}
