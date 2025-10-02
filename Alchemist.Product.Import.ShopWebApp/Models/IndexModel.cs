using System.Text.Json.Serialization;

namespace Alchemist.Product.ShopWebApp.Models;

internal class IndexModel
{
    public bool ShopsUploaded { get; set; } = false;

    [JsonInclude]
    internal ShopTabModel ShopTab { get; set; }
}
