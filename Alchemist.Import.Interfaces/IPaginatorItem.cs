namespace Alchemist.Import.Interfaces;

public interface IPaginatorItem
{
    string GetPageUrl(string urlFormat, int page);

    string GetNextPageUrl(string urlFormat, int page);
}
