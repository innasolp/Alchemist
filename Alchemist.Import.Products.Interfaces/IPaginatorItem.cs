namespace Alchemist.Import.Products.Interfaces;

public interface IPaginatorItem
{
    string GetPageUrl(string urlFormat, string item, int page);

    string GetNextPageUrl(string urlFormat, string item, int page);
}
