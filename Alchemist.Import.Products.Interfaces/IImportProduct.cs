using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Products.Interfaces;

public interface IImportProductItem : IItem
{
    string ShopName { get; }    
}
