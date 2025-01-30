using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public enum WebCharacteristicType
{
    Unknown,
    Sku,
    Type,
    Brand,
    SkinType,
    SkinEffect,
    AreaUseHealthandBeauty,
    Country,
    Material,
    Composition
}

public class WebCharacteristics : IJsonOnDeserialized
{
    [JsonPropertyName("productTitle")]
    public string ProductTitle { get; set; }

    [JsonPropertyName("characteristics")]
    public WebCharacteristic[] Characteristics { get; set; }


    [JsonIgnore]
    public string Brand { get; set; }

    [JsonIgnore]
    public string Type { get; set; }

    [JsonIgnore]
    public string Sku { get; set; }

    [JsonIgnore]
    public string[] PurposeTypes { get; set; }

    [JsonIgnore]
    public string[] Components { get; set; }

    [JsonIgnore]
    public string Country { get; set; }

    public void OnDeserialized()
    {
        var brandCharacteristicItem = Characteristics.SelectMany(c => c.AllCharacteristicItems).FirstOrDefault(cv => cv.WebCharacteristicType == WebCharacteristicType.Brand);
        if (brandCharacteristicItem != null)
            Brand = brandCharacteristicItem.Values[0].Text;

        var typeCharacteristicItem = Characteristics.SelectMany(c => c.AllCharacteristicItems).FirstOrDefault(cv => cv.WebCharacteristicType == WebCharacteristicType.Type);
        if (typeCharacteristicItem != null)
            Type = typeCharacteristicItem.Values[0].Text;

        var skuCharacteristicItem = Characteristics.SelectMany(c => c.AllCharacteristicItems).FirstOrDefault(cv => cv.WebCharacteristicType == WebCharacteristicType.Sku);
        if (skuCharacteristicItem != null)
            Sku = skuCharacteristicItem.Values[0].Text;

        var purposeTypeCharacteristicItem = Characteristics.SelectMany(c => c.AllCharacteristicItems).FirstOrDefault(cv => cv.WebCharacteristicType == WebCharacteristicType.SkinEffect);
        PurposeTypes = purposeTypeCharacteristicItem != null
                ? purposeTypeCharacteristicItem.Values.Select(v => v.Text).ToArray()
                : [];

        var countryCharacteristicItem = Characteristics.SelectMany(c => c.AllCharacteristicItems).FirstOrDefault(cv => cv.WebCharacteristicType == WebCharacteristicType.Country);
        if (countryCharacteristicItem != null)
            Country = countryCharacteristicItem.Values[0].Text;

        var componentsCharacteristicItems = Characteristics.SelectMany(c => c.AllCharacteristicItems).
            Where(ci => ci.WebCharacteristicType == WebCharacteristicType.Material || ci.WebCharacteristicType == WebCharacteristicType.Composition).ToList();
        if(componentsCharacteristicItems.Count > 0)
        {
            Components = componentsCharacteristicItems.SelectMany(c => c.Values).Select(v => v.Text).ToArray();
        }
    }
}

public class WebCharacteristic : IJsonOnDeserialized
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("short")]
    public WebCharacteristicItem[] ShortCharacteristicItems { get; set; }

    [JsonPropertyName("long")]
    public WebCharacteristicItem[] LongCharacteristicItems { get; set; }

    [JsonIgnore]
    public WebCharacteristicItem[] AllCharacteristicItems { get; set; }

    void IJsonOnDeserialized.OnDeserialized()
    {
        var items = new List<WebCharacteristicItem>();
        if(ShortCharacteristicItems != null) items.AddRange(ShortCharacteristicItems);
        if (LongCharacteristicItems != null) items.AddRange(LongCharacteristicItems);
        AllCharacteristicItems = [.. items];
    }
}

public class WebCharacteristicItem : IJsonOnDeserialized
{
    private static Dictionary<string, WebCharacteristicType> WebCharacteristicTypes;
    static WebCharacteristicItem()
    {
        WebCharacteristicTypes = Enum.GetValues(typeof(WebCharacteristicType)).Cast<WebCharacteristicType>().
            ToDictionary(k => k.ToString(), v => v);
    }

    [JsonIgnore]
    public WebCharacteristicType WebCharacteristicType { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("values")]
    public WebCharacteristicItemValue[] Values { get; set; }

    public void OnDeserialized()
    {
        var characteristicType = WebCharacteristicTypes.FirstOrDefault(wct => wct.Key == Key.Replace("_", ""));
        WebCharacteristicType = characteristicType.Key != null ? characteristicType.Value : WebCharacteristicType.Unknown;
    }
}

public class WebCharacteristicItemValue
{

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
