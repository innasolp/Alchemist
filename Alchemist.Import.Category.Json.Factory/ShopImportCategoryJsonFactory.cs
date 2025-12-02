using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Import.Html.Factory;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Alchemist.Import.Settings.Category;

namespace Alchemist.Import.Factory.Category.Json;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesTimerService> logger,
    ILoaderServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null) 
    : ImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly ICategoryItemHandler _itemHandler = itemHandler;

    public override Type ServiceImplementationType => typeof(ShopImportCategoriesTimerService);    

    protected override IImportService Create(ILogger logger, 
        string name,
        IImportSource shopModel,
        IShopImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        if (shopModel is not ICategoryShopModel categoryShopModel)
            throw new InvalidOperationException($"Invalid type {shopModel.GetType()}. Must be implementation of {typeof(ICategoryShopModel)}");

        var loadOptionsService = (shopImportSettings.GetService(nameof(CategoryLoadOptions))?.Value) 
            ?? throw new InvalidDataException($"CategoryLoadOptions not exists for {shopImportSettings.ShopName}");
        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(loadOptionsService);
        
        var htmlSearchOptionsService = shopImportSettings.GetService(nameof(HtmlSearchFactoryOptions))?.Value;
        var htmlSearchFactoryOptions = htmlSearchOptionsService != null ? JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(htmlSearchOptionsService) : null;

        var htmlSearcher = htmlSearchFactoryOptions != null 
            ? HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions.SearchMatchType, htmlSearchFactoryOptions.SearchElementType)
            : null;

        return new ShopImportCategoriesTimerService(logger as ILogger<ShopImportCategoriesTimerService>, 
            name,
            htmlSearcher,
            browserService,
            categoryShopModel.CategorySourceUrl,
            categoryShopModel.Name,
            categoryShopModel.Url,
            categoryLoadOptions, _itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IShopImportSettings shopImportSettings)
    {
        if (logger is ILogger<ShopImportCategoriesTimerService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else 
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
