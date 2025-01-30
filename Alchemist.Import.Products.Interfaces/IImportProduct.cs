using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Products.Interfaces;

public interface IImportProduct : IProcessedItem
{
    string Name { get; }

    string Url { get; }

    string ShopName { get; }    
}
