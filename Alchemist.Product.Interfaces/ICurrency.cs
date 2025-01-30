namespace Alchemist.Product.Interfaces;

public interface ICurrency
{
    public short Id { get; set; }
    public short? Code { get; set; }
    public string Name { get; set; }
    public string? FullName { get; set; }
}
