namespace Alchemist.Product.Data;

public partial class Currency
{
    public short Id { get; set; }
    public short? Code { get; set; }
    public string  Name { get; set; }
    public string?  FullName { get; set; }
}
