using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings;

[JsonConverter(typeof(JsonStringEnumConverter<UrlFormatType>))]
public enum UrlFormatType
{
    Url = 0,
    ItemId = 1,
    UrlWithItemId = 2
}