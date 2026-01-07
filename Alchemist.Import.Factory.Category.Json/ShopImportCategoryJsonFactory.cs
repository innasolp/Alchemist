using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Category.Service.Json;
using Alchemist.Import.Settings.Category;
using Import.Factory.Interfaces;
using Import.Html;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Alchemist.Import.Factory.Category.Json;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesJsonTimerService> logger,
    ILoaderServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null) 
    : ShopImportCategoryFactory<JsonElement>(logger, browserServiceFactory, itemHandler, logFactory)
{
    public override Type ServiceImplementationType => typeof(ShopImportCategoriesJsonTimerService);

    protected override ShopImportCategoriesTimerService<JsonElement> CreateShopImportCategoriesTimerService(ILogger logger, string name,
        IHtmlSearcher htmlSearcher, 
        ILoaderService loaderService, 
        ICategoryShopSource categoryShopModel,
        ICategoryShopImportSettings categoryShopImportSettings,
        CategoryLoadOptions categoryLoadOptions, 
        ICategoryItemHandler categoryItemHandler)
    {
        if (logger is not ILogger<ShopImportCategoriesJsonTimerService> categoryJsonLogger)
            throw new InvalidDataException(logger.GetType().FullName);

        return new ShopImportCategoriesJsonTimerService(categoryJsonLogger,
            name,
            htmlSearcher,
            loaderService,
            categoryShopImportSettings.CategorySourceUrl,
            categoryShopModel.Name,
            categoryShopModel.Url,
            categoryLoadOptions,
            categoryItemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IImportSettings shopImportSettings)
    {
        if (logger is ILogger<ShopImportCategoriesJsonTimerService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}