using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.Factory.Category;

internal class ShopImportCategoriesSimpleService(ILogger logger,
    string name,
    ILoaderService loader, 
    string categorySourceUrl, 
    string sourceName, 
    string url, 
    IEnumerable<ICategoryLoader> categoryLoadStages,
    ICategoryItemHandler itemHandler, 
    object? categoryLoadData = null)
    : ShopImportCategoriesService(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
{
}