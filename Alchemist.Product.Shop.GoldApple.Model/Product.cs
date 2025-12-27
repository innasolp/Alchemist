using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.GoldApple.Model;

public abstract class ProductBase
{
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; }

    [JsonPropertyName("mainVariantItemId")]
    public string MainVariantItemId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("productType")]
    public string ShortDescription { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("brand")]
    public string Brand { get; set; }
}

public class ProductCardItem
{
    [JsonPropertyName("product")]
    public ProductInCategory Product { get; set; }
}

public class ProductInCategory : ProductBase, IJsonOnDeserialized, ICategoryProductItem
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    string ICategoryProductItem.Id => ItemId;

    string ICategoryProductItem.ItemUrl => Url;

    [JsonPropertyName("price")]
    public ProductItemPrice ProductItemPrice { get; set; }

    [JsonIgnore]
    public string Currency { get; set; }

    [JsonIgnore]
    public double Price { get; set; }

    public void OnDeserialized()
    {
        var priceItemData = ProductItemPrice.Actual ?? ProductItemPrice.Regular ?? ProductItemPrice.Loyalty ?? ProductItemPrice.Old;
        if (priceItemData != null)
        {
            Price = priceItemData.Amount;
            Currency = priceItemData.Currency;
        }
    }
    string ICategoryProductItem.Name => $"{Name} {ShortDescription} {Brand}";

    int ICategoryProductItem.CategoryItemId { get; set; }
}
public class ProductItemPrice
{
    [JsonPropertyName("regular")]
    public ProductItemPriceData Regular { get; set; }

    [JsonPropertyName("loyalty")]
    public ProductItemPriceData Loyalty { get; set; }

    [JsonPropertyName("actual")]
    public ProductItemPriceData Actual { get; set; }

    [JsonPropertyName("old")]
    public ProductItemPriceData Old { get; set; }
}

public class ProductItemPriceData
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }
}


public class ProductData : IProductItem
{
    [JsonPropertyName("data")]
    public Product Data { get; set; }

    string IProductItem.Comment => Data.Description;

    string IProductItem.Articul => Data.ItemId;

    string IProductItem.ItemId => Data.ItemId;

    string IProductItem.Name => Data.Name;

    string[] IProductItem.Components => Data.Components;

    string IProductItem.Brand { get => Data.Brand; set { Data.Brand = value; } }

    string? IProductItem.Country => Data.Country;

    string IProductItem.ProductType => Data.ProductType;

    string[] IProductItem.Purposes => Data.Purposes;

    string IProductItem.Currency { get; set; }
    double IProductItem.Price { get; set; }
    string IProductItem.Url { get; set; }

    string IProductItem.ApiUrl { get; set; }
    int IProductItem.CategoryId { get; set; }
}

public class Product : ProductBase, IJsonOnDeserialized
{
    [JsonPropertyName("productDescription")]
    public ProductDescription[] ProductDescriptions { get; set; }

    [JsonIgnore]
    public string[] Components { get; set; }

    [JsonIgnore]
    public string Description { get; set; }

    [JsonIgnore]
    public string? Country { get; set; }

    [JsonIgnore]
    public string ProductType { get; set; }

    [JsonIgnore]
    public string[] Purposes { get; set; }

    public void OnDeserialized()
    {
        var componentDescription = ProductDescriptions.FirstOrDefault(pd => pd.ProductDescriptionType == ProductDescriptionType.Components);
        if (componentDescription != null && !string.IsNullOrWhiteSpace(componentDescription.Content))
            Components = componentDescription.Content.Split(", ").Where(c => c.Length < 100).ToArray();
        else
            Components = [];

        var description = ProductDescriptions.FirstOrDefault(pd => pd.ProductDescriptionType == ProductDescriptionType.Description);
        if (description != null && !string.IsNullOrWhiteSpace(description.Content))
        {
            Description = description.Content;
            if (description.Attributes != null && description.Attributes.Any())
            {
                ProductType = description.Attributes[(int)ProductDescriptionAttributeType.ProductType].Value.Trim();
                Purposes = (description.Attributes.Length > 2)
                    ? [.. description.Attributes[(int)ProductDescriptionAttributeType.Purpose].Value.Split(",").Select(s=>s.Trim())]
                    : [];
            }
            else Purposes = [];
        }

        var brand = ProductDescriptions.FirstOrDefault(pd => pd.ProductDescriptionType == ProductDescriptionType.Brand);
        if (brand != null && !string.IsNullOrWhiteSpace(brand.Title))
        {
            Country = brand.Subtitle.Trim();
        }
    }
}

