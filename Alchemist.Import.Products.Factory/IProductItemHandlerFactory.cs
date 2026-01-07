using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;


namespace Alchemist.Import.Factory.Products;

public interface IProductItemHandlerFactory
{
    IProductItemHandler Create(IImportSettings importSettings);
}