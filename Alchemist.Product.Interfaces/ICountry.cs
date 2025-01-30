namespace Alchemist.Product.Interfaces;

public interface ICountry
{
    public short Id { get; set; }

    public string Name { get; set; }

    public string? Transcript { get; set; }
}
