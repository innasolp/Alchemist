namespace Alchemist.Product.Interfaces;

public interface IPurposeComponentGroup
{
    public short ComponentGroupId { get; set; }
    public short PurposeTypeId { get; set; }
    public string? Comment { get; set; }
}
