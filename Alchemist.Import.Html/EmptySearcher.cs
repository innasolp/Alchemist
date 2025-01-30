namespace Alchemist.Import.Html;

internal class EmptySearcher : IHtmlSearcher
{
    public Task<List<string>> GetValues(Stream html, HtmlSearchOptions htmlSearchOptions)
    {
        throw new NotImplementedException();
    }
}
