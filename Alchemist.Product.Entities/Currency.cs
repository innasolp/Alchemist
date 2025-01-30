using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class Currency:ICurrency
{
    public short Id { get; set; }
    public short? Code { get; set; }
    public string Name { get; set; }
    public string? FullName { get; set; }
}
