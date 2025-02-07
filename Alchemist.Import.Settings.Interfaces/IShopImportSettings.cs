using WebLoader.Common;

namespace Alchemist.Import.Settings.Interfaces;

public interface IShopImportSettings
{
    int Id { get; set; }

    string Name { get; set; }

    string? Caption { get; set; }

    int ShopId { get; set; }

    IImportServiceSettings ImportService { get; }

    string? Url { get; }

    IImportServiceSettings? RequestHeadersSettings { get; }

    RequestHeaders? RequestHeaders { get; }

    IImportServiceSettings WebLoader { get; }

    IImportServiceSettings? BrowserDataLoader { get; }

    public bool? Perfomance { get; }

    IEnumerable<IImportServiceSettings> Services { get; }
}
