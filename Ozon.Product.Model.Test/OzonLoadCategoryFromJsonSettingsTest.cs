using Alchemist.Import.Products.Json;
using Json.CustomSerialization;
using System.Text.Json.Nodes;
using Product.Import.Json.Test.Common;
using Alchemist.Import.Product.JsonPathHandlers;

namespace Ozon.Product.Model.Test;

public class OzonLoadCategoryFromJsonSettingsTest
{
    [Fact]
    public void LoadOzonCategoryFromJson()
    {
        var category = TestHelper.GetItemFromJson<CategoryProducts>("Content/ozon.category.json", "Content/ozon.category.settings.json");

        Assert.NotNull(category);
        Assert.True(category.TotalCount > 0);
        Assert.Equal(12, category.CategoryProductItems.Length); 
        Assert.True(category.CategoryProductItems.All(c=>c != null));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.Name)));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.Id)));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.ItemUrl)));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.Brand)));
    }

    [Fact]
    public void LoadOzonCategoryItemPricesFromJson()
    {
        var jsonSettings = new JsonSettings
        {
            PropertyNodePathes = new Dictionary<string, string[]>()
            {
                { "CategoryProductItems", ["widgetStates/tileGridDesktop%/.value/items"] }
            },
            ItemJsonSettings = new Dictionary<string, JsonSettings>
            {                
                { "CategoryProductItems", new JsonSettings()
                    {
                        PropertyNodePathes = new Dictionary<string, string[]> ()
                        {
                            { "Price", ["mainState/[type=priceV2]/priceV2/price/[textStyle=PRICE]/text/.price"] }
                        }
                    }
                }
            }
        };
        var jsonLoader = new JsonLoader(jsonSettings);
        jsonLoader.AddPathHandler(new PriceJsonPathHandler());

        var json = "Content/ozon.category.json".ReadFromJsonFile<JsonObject>();

        var category = new CategoryProducts();
        jsonLoader.LoadFromJson(category, json);
        
        Assert.NotNull(category);
        Assert.Equal(12, category.CategoryProductItems.Length); 
        Assert.True(category.CategoryProductItems.All(c=>c != null));
        Assert.True(category.CategoryProductItems.All(c=>c.Price > 0));
    }

    [Fact]
    public void LoadOzonCategoryItemCurrencyFromJson()
    {
        var jsonSettings = new JsonSettings
        {
            PropertyNodePathes = new Dictionary<string, string[]>()
            {
                { "CategoryProductItems", ["widgetStates/tileGridDesktop%/.value/items"] }
            },
            ItemJsonSettings = new Dictionary<string, JsonSettings>
            {
                { "CategoryProductItems", new JsonSettings()
                    {
                        PropertyNodePathes = new Dictionary<string, string[]> ()
                        {
                            { "Currency", ["mainState/[type=priceV2]/priceV2/price/[textStyle=PRICE]/text/.currency"] }
                        }
                    }
                }
            }
        };
        var jsonLoader = new JsonLoader(jsonSettings);
        jsonLoader.AddPathHandler(new CurrencyJsonPathHandler());

        var json = "Content/ozon.category.json".ReadFromJsonFile<JsonObject>();

        var category = new CategoryProducts();
        jsonLoader.LoadFromJson(category, json);

        Assert.NotNull(category);
        Assert.Equal(12, category.CategoryProductItems.Length);
        Assert.True(category.CategoryProductItems.All(c => c != null));
        Assert.True(category.CategoryProductItems.All(c => !string.IsNullOrEmpty(c.Currency)));
    }
}