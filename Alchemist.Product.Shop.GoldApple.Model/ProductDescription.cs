using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.GoldApple.Model;

public enum ProductDescriptionType
{
    Description = 0,
    Usage = 1,
    Components = 2,
    Brand = 3,
    AdditionalInfo = 4,
    Unknown = -1
}

public enum ProductDescriptionAttributeType
{
    ProductType = 0,
    Gender = 1,
    Purpose = 2,
    AkinType = 3,
    BodyPart = 4,
    Amount = 5,
    Unknown = -1
}

public class DescriptionAttribute
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

public class ProductDescription : IJsonOnDeserialized
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonIgnore]
    public ProductDescriptionType ProductDescriptionType { get; set; }

    [JsonPropertyName("attributes")]
    public DescriptionAttribute[] Attributes { get; set; }

    public void OnDeserialized()
    {
        ProductDescriptionType = ProductDescriptionType.Unknown;

        if (Value.Contains("Description"))
            ProductDescriptionType = ProductDescriptionType.Description;

        if (Value.Contains("Brand"))
            ProductDescriptionType = ProductDescriptionType.Brand;

        if (Value.Contains("Text") && Text != null)
        {
            if (Text.ToLower().Contains("состав"))
                ProductDescriptionType = ProductDescriptionType.Components;

            if (Text.ToLower().Contains("применение"))
                ProductDescriptionType = ProductDescriptionType.Usage;

            if (Text.ToLower().Contains("дополнительная информация"))
                ProductDescriptionType = ProductDescriptionType.AdditionalInfo;
        }
    }
}


