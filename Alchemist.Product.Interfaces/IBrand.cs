namespace Alchemist.Product.Interfaces;

public interface IBrand
{
    public int Id { get; set; }

    public string Name { get; set; }

    public short? CountryId { get; set; }

    public string? Comment { get; set; }
}
