using Alchemist.Import.Products.Interfaces;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Products.Json;

public class PagingCategoryProducts : CategoryProducts, IPagingItem
{
    private static readonly ReadOnlyDictionary<string, Type> _propertyTypes = new(new Dictionary<string, Type>()
    {
          { nameof(PrevPage), typeof(string) },
          { nameof(NextPage), typeof(string) }
    });

    public override IReadOnlyDictionary<string, Type> PropertyTypes => _propertyTypes.Union(base.PropertyTypes).ToDictionary();

    string? IPagingItem.GetNextPageUrl(string urlFormat, string item, int page)
    {
        return string.Format(urlFormat, System.Web.HttpUtility.UrlEncode(NextPage));
    }

    string? IPagingItem.GetPageUrl(string urlFormat, string item, int page)
    {
        return string.Format(urlFormat, System.Web.HttpUtility.UrlEncode(PrevPage));
    }

    public string? PrevPage
    {
        get => GetPropertyValue<string?>(nameof(PrevPage));
        set => SetPropertyValue(nameof(PrevPage), value);
    }

    public string? NextPage
    {
        get => GetPropertyValue<string?>(nameof(NextPage));
        set => SetPropertyValue(nameof(NextPage), value);
    }
}