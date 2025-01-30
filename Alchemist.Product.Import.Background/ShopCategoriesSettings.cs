using Alchemist.Import.Html;

namespace Alchemist.Product.Import.Background;

public class ShopCategoriesSettings : ShopSettings
{
    public ServiceValueSettings? CategoryLoadOptions { get; set; }

    public ServiceValueSettings? HtmlSearchFactory { get; set; }

    public HtmlSearchSettings HtmlSearchSettings { get; set; }
}

public class HtmlSearchSettings
{
    public SearchMatchType SearchMatchType { get; set; }

    public SearchElementType SearchElementType { get; set; }
}
