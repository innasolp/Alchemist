using Import.Html.Factory;
using ShopImport.Category.Loader.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace ShopImport.Category.RecursiveHtmlLoader;

internal class RecursiveJsonCategoryFromHtmlLoaderFactory : ICategoryLoaderFactory
{
    ICategoryLoader ICategoryLoaderFactory.CreateCategoryLoader(ICategoryLoadOptions categoryLoadStageOptions)
    {
        if (categoryLoadStageOptions is not HtmlCategoryLoadOptions htmlCategoryLoadStageOptions)
            throw new ArgumentException($"Expected {nameof(HtmlCategoryLoadOptions)} but got {categoryLoadStageOptions.GetType().Name}");
        
        return CreateCategoryLoader(htmlCategoryLoadStageOptions);       
    }

    public static RecursiveJsonCategoryFromHtmlLoader CreateCategoryLoader(HtmlCategoryLoadOptions categoryLoadOptions)
    {
        var htmlSearcher =  HtmlSearchFactory.CreateSearcher(categoryLoadOptions.HtmlSearchFactoryOptions.SearchMatchType, 
            categoryLoadOptions.HtmlSearchFactoryOptions.SearchElementType);

        return new RecursiveJsonCategoryFromHtmlLoader(htmlSearcher, categoryLoadOptions);
    }

    ICategoryLoader ICategoryLoaderFactory.CreateCategoryLoader(JsonObject categoryLoadStageOptions)
    {
        return CreateCategoryLoader(categoryLoadStageOptions);
    }

    public static RecursiveJsonCategoryFromHtmlLoader CreateCategoryLoader(JsonObject categoryLoadStageOptionsJson)
    {
        var categoryLoadStageOptions = JsonSerializer.Deserialize<HtmlCategoryLoadOptions>(categoryLoadStageOptionsJson.ToJsonString());
        return categoryLoadStageOptions == null
            ? throw new ArgumentException("Failed to deserialize HtmlCategoryLoadStageOptions from JsonObject")
            : CreateCategoryLoader(categoryLoadStageOptions);
    }
}