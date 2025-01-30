using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class PurposeType:IPurposeType
{
    public short Id { get; set; }

    public string Name { get; set; }
}
