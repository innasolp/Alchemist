using Alchemist.Product.Interfaces;
using Alchemist.Product.Model;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopModelFormBinder(ILogger<ModelFormBinder> logger) : ModelFormBinder(logger)
{
    protected override string[] PropertyNames => [ nameof(IShop.Id), nameof(IShop.Name), nameof(IShop.Url), nameof(IShop.Caption)];

    protected override object? GetValueByProperties(Dictionary<string, string?> propertyValues)
    {
        var shop = new ShopModel
        {
            Id = propertyValues.TryGetValue(nameof(IShop.Id), out var idValue) && int.TryParse(idValue, out var shopId)
                ? shopId
                : 0,

            Name = propertyValues.TryGetValue(nameof(IShop.Name), out var nameValue) && !string.IsNullOrEmpty(nameValue)
                ? nameValue :
                throw new InvalidOperationException($"shop name is empty"),

            Url = propertyValues.TryGetValue(nameof(IShop.Url), out var urlValue) && !string.IsNullOrEmpty(urlValue)
                ? urlValue :
                throw new InvalidOperationException($"shop url is empty"),

            Caption = propertyValues.TryGetValue(nameof(IShop.Caption), out var captionValue) ? captionValue : null
        };

        return shop;
    }
}
