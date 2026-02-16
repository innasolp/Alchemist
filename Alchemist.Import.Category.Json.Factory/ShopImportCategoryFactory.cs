using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Settings.Category;
using Alchemist.Import.Settings.Extensions;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Import.Interfaces;
using Import.LoaderSettings;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Factory.Category;

public class ShopImportCategoryFactory(ILogger<ShopImportCategoriesTimerService> logger,
    ILoaderServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler, 
    IEnumerable<ICategoryLoaderFactory> categoryLoadStageFactories,
    IImportServiceLogFactory? logFactory = null) 
    : ImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly ICategoryItemHandler _itemHandler = itemHandler;

    public override Type ServiceImplementationType => typeof(ShopImportCategoriesTimerService);

    protected override IImportService Create(ILogger logger, 
        string name,
        IImportSource shopModel,
        IImportSettings shopImportSettings,
        ILoaderService browserService) 
    {
        if (shopImportSettings is not ICategoryShopImportSettings categoryShopImportSettings)
            throw new InvalidDataException($"Invalid settings type {shopImportSettings.GetType().Name}");

        if (shopModel is not ICategoryShopSource categoryShopModel)
            throw new InvalidOperationException($"Invalid type {shopModel.GetType()}. Must be implementation of {typeof(ICategoryShopSource)}");

        var categoryDataService = shopImportSettings.GetService("RequestOptions");
        var categoryLoadData = categoryDataService?.GetServiceValue<RequestOptions>();

        var categoryImportOptionsService = shopImportSettings.GetService(nameof(CategoryImportOptions));
        var categoryImportOptions = categoryImportOptionsService?.GetServiceValue<CategoryImportOptions>();
        
        var stagesService = shopImportSettings.GetService("LoadStages")
            ?? throw new InvalidDataException($"LoadStages not exists for {shopModel.Name}");
        var jsonStages = stagesService.GetServiceValue<JsonObject[]>();
        
        var stages = jsonStages.Select(CreateCategoryLoadStages);

        return new ShopImportCategoriesTimerService(logger,
            name,
            browserService,
            categoryShopImportSettings.CategorySourceUrl,
            categoryShopImportSettings.ShopName,
            categoryShopImportSettings.ShopUrl,
            stages,
            _itemHandler,
            categoryImportOptions,
            categoryLoadData);
    }

    protected ICategoryLoader CreateCategoryLoadStages(JsonObject stageSettingsJson)
    {
        var stageSettings = JsonSerializer.Deserialize<LoadStageSettings>(stageSettingsJson.ToJsonString())
            ?? throw new InvalidDataException("Cannot deserialize LoadStageSettings");

        var stageFactory = categoryLoadStageFactories.FirstOrDefault(f => f.GetType().Name.Contains(stageSettings.Type))
            ?? throw new InvalidDataException($"Cannot find category load stage factory for type {stageSettings.Type}");

        return stageFactory.CreateCategoryLoader(stageSettings.CategoryLoadOptions);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IImportSettings shopImportSettings)
    {
        if (logger is ILogger<ShopImportCategoriesTimerService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}