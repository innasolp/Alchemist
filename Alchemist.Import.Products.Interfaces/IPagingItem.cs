namespace Alchemist.Import.Products.Interfaces;

public interface IPagingItem
{
    string? GetPageUrl(string urlFormat, string item, int page);

    string? GetNextPageUrl(string urlFormat, string item, int page);
}
