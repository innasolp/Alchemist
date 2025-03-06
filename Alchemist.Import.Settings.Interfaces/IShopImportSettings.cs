namespace Alchemist.Import.Settings.Interfaces;

public interface IShopImportSettings
{
    int Id { get; set; }

    string Name { get; set; }

    string? Caption { get; set; }

    int ShopId { get; set; }

    IImportServiceSettings ImportService { get; set; }

    string? Url { get; set; }

    IImportServiceSettings? RequestHeaders { get; set; }

    IImportServiceSettings WebLoader { get; set; }

    IImportServiceSettings? BrowserDataLoader { get; set; }

    public bool? Perfomance { get; set; }

    IList<IImportServiceSettings> Services { get; }
}
