using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Background;

public class ServiceSetting : IJsonOnDeserialized
{
    public string Name { get; set; }

    public string Path { get; set; }

    public string Host { get; set; }

    public void OnDeserialized()
    {

    }

    public void OnDeserializing()
    {
        //todo
    }
}
