using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class Brand : IBrand
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public short CountryId { get; set; }

    public string? Comment { get; set; }
}
