using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class WebDescription : IJsonOnDeserialized
{
    private readonly string[] _componentSeparators = { ",", " • ", ";" };

    [JsonPropertyName("characteristics")]
    public DescriptionCharacteristic[] DescriptionCharacteristics { get; set; }

    [JsonIgnore]
    public string[] Components { get; set; }

    public void OnDeserialized()
    {
        var componentCharacteristic = DescriptionCharacteristics.FirstOrDefault(c => c.CharacteristicType == DescriptionCharacteristicType.Components);
        if (componentCharacteristic != null)
        {
            var currentSeparator = _componentSeparators.Where(componentCharacteristic.Content.Contains).FirstOrDefault();
            Components = currentSeparator != null ? componentCharacteristic.Content.Split(currentSeparator) : ([componentCharacteristic.Content]);
        }
    }
}


