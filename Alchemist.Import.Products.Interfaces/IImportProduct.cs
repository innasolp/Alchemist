using Import.Interfaces;

namespace Alchemist.Import.Products.Interfaces;

public interface IImportProductItem
{
    string Name { get; }

    string Url { get; }

    string ShopName { get; }    
}
