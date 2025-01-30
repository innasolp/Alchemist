using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Background;

public abstract class ShopSettings 
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string Url { get; set; }
    public string RequestHeadersPath { get; set; }
    public string ServiceProviderPath { get; set; }

    [JsonPropertyName("WebLoader")]
    public ServiceSetting WebLoader { get; set; }

    [JsonPropertyName("BrowserDataLoader")]
    public ServiceSetting BrowserDataLoader { get; set; }

    public string Id { get; set; }

    public bool? Perfomance { get; set; }
}
