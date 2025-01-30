using Alchemist.Import.Products.Interfaces;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class ProductItem : ICategoryProductItem, IJsonOnDeserialized
{
    private const string productLink = "/product/";
    private const string startParams = "/?";

    [JsonPropertyName("action")]
    public Action Action { get; set; }

    [JsonPropertyName("skuId")]
    public string SkuId { get; set; }

    [JsonIgnore]
    public string Name { get; set; }

    [JsonPropertyName("mainState")]
    public MainState[] MainState {  get; set; }

    string ICategoryProductItem.Id => SkuId;

    string ICategoryProductItem.ItemUrl => Action.Link;

    string ICategoryProductItem.Currency => "RUB";

    [JsonIgnore]
    public double Price { get; set; }

    public void OnDeserialized()
    {
        if (!string.IsNullOrEmpty(Action.Link))
        {
            var substr = Action.Link.Replace(productLink, "");
            var startParamsIndex = substr.IndexOf(startParams);
            Name = substr.Substring(0, startParamsIndex);
        }

        var priceMainState = MainState.FirstOrDefault(m => m.Atom.PriceV2 != null);
        if (priceMainState != null)
        {
            var priceItem = priceMainState.Atom.PriceV2.PriceItems.FirstOrDefault(i => i.PriceTextStyle == PriceTextStyle.Price) ?? priceMainState.Atom.PriceV2.PriceItems.FirstOrDefault();
            if (priceItem != null)
                Price = priceItem.Price;
        }
    }
}

public class Action
{
    [JsonPropertyName("link")]
    public string Link { get; set; }
}

public class MainState
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("atom")]
    public Atom Atom { get; set; }
}

public class Atom
{
    [JsonPropertyName("type")]
    public string Type {  get; set; }

    [JsonPropertyName("priceV2")]
    public PriceV2 PriceV2 { get; set; }
}

public class PriceV2
{
    [JsonPropertyName("price")]
    public PriceItem[] PriceItems { get; set; }
}

public enum PriceTextStyle
{
    Price,
    OriginalPrice
}

public class PriceItem: IJsonOnDeserialized
{
    [JsonIgnore]
    public PriceTextStyle PriceTextStyle { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("textStyle")]
    public string TextStyle {  get; set; }

    [JsonIgnore]
    public double Price {  get; set; }

    private const string RubSymbol = "₽";

    public void OnDeserialized()
    {
        switch (Text.ToUpper())
        {
            case "PRICE":
                PriceTextStyle = PriceTextStyle.Price; break;

            case "ORIGINAL_PRICE":
                PriceTextStyle = PriceTextStyle.OriginalPrice; break;

        }

        var trimmedPrice = Text.Replace(RubSymbol, "").Trim();
        var priceSymbols = new List<char>();
        foreach (char c in trimmedPrice)
        {
            if (char.IsWhiteSpace(c)) continue;
            priceSymbols.Add(c);
        }
        var priceStr = new string(priceSymbols.ToArray());
        if (double.TryParse(priceStr, new System.Globalization.NumberFormatInfo() { CurrencyDecimalSeparator = "." }, out double price))
            Price = price;
    }
}


