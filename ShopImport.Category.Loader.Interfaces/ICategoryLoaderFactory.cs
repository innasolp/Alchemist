using System.Text.Json.Nodes;

namespace ShopImport.Category.Loader.Interfaces;

public interface ICategoryLoaderFactory
{
    ICategoryLoader CreateCategoryLoader(ICategoryLoadOptions categoryLoadStageOptions);

    ICategoryLoader CreateCategoryLoader(JsonObject categoryLoadStageOptions);
}
