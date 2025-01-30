using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class PurposeComponentGroup : IPurposeComponentGroup
{
    public short ComponentGroupId { get; set; }
    public short PurposeTypeId { get; set; }
    public string? Comment { get; set; }
}
