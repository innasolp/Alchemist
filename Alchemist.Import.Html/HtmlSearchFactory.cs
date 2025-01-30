namespace Alchemist.Import.Html;

public class HtmlSearchFactory : IHtmlSearchFactory
{
    public IHtmlSearcher CreateSearcher(SearchMatchType searchMatchType, SearchElementType searchElementType = SearchElementType.Attribute)
    {
        if (searchElementType == SearchElementType.Attribute)
            switch (searchMatchType)
            {
                case SearchMatchType.Equals:
                    return new EqualsAttributeHtmlSearcher();

                case SearchMatchType.Like:
                    return new LikeAttributeHtmlSearcher();

                default:
                    return new EmptySearcher();
            }
        else
            switch (searchMatchType)
            {
                case SearchMatchType.Equals:
                    return new EqualsByJsonValueHtmlSearcher();

                case SearchMatchType.Like:
                    return new LikeAttributeHtmlSearcher();

                default:
                    return new EmptySearcher();
            }
    }
}
