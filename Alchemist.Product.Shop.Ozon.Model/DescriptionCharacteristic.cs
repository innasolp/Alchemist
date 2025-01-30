using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public enum DescriptionCharacteristicType
{
    Components = 1,
    Usage = 2,
    Unknown = 0
}

public class DescriptionCharacteristic : IJsonOnDeserialized
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }


    [JsonIgnore]
    public DescriptionCharacteristicType CharacteristicType { get; set; }

    public void OnDeserialized()
    {
        switch (Title.ToLower())
        {
            case "состав":
                CharacteristicType = DescriptionCharacteristicType.Components; break;

            case "способ применения":
                CharacteristicType = DescriptionCharacteristicType.Usage; break;

        }
    }
}
