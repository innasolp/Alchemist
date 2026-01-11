using System.Text.Json.Serialization;

namespace Alchemist.Import.Products.Interfaces;

[JsonConverter(typeof(JsonStringEnumConverter<PathFormatType>))]
public enum PathFormatType
{
    None = 0,
    Path = 1,
    ItemId = 2,
    PathWithItemId = 3
}