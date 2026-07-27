using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class Country : ICountry
{
    public short Id { get; set; }

    public string Name { get; set; }

    public string? Transcript { get; set; }
}