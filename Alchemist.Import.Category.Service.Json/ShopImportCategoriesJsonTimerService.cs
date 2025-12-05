using Alchemist.Import.Category.Interfaces;
using Import.Html;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Alchemist.Import.Category.Service.Json;

public class ShopImportCategoriesJsonTimerService : ShopImportCategoriesTimerService<JsonElement>
{
    public ShopImportCategoriesJsonTimerService(ILogger<ShopImportCategoriesJsonTimerService> logger,
        string name,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
        CategoryLoadOptions categoryLoadOptions, 
        ICategoryItemHandler itemHandler) 
        : base(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadOptions, itemHandler)
    {
    }

    public ShopImportCategoriesJsonTimerService(ILogger<ShopImportCategoriesJsonTimerService> logger, 
        string name, 
        IHtmlSearcher? htmlSearcher, 
        ILoaderService loader, 
        string categorySourceUrl, 
        string sourceName, 
        string url, 
        CategoryLoadOptions categoryLoadOptions,
        ICategoryItemHandler itemHandler) 
        : base(logger, name, htmlSearcher, loader, categorySourceUrl, sourceName, url, categoryLoadOptions, itemHandler)
    {
    }

    protected override IElementHelper<JsonElement> GetElementHelper()
    {
        return new JsonElementHelper();
    }

    protected override async Task<JsonElement> LoadElementFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken : cancellationToken);
            return jsonDocument.RootElement;
        }
        catch (JsonException e)
        {
            throw new SerializationException(e.Message, e);
        }
    }

    protected override JsonElement LoadElementFromString(string str)
    {
        try
        {
            return JsonDocument.Parse(str).RootElement;
        }
        catch(JsonException e)
        {
            throw new SerializationException(e.Message, e);
        }
    }
}