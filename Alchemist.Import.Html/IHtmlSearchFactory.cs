namespace Alchemist.Import.Html;

public interface IHtmlSearchFactory
{
    IHtmlSearcher CreateSearcher(SearchMatchType searchType, SearchElementType searchElementType);
}
