using Alchemist.Import.Settings.Interfaces;
using System.Collections;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Test.Model;

public abstract class TestShopImportSettings : IShopImportSettings
{
    public TestImportServiceSettings ImportService { get; set; }

    public TestImportServiceSettings? RequestHeaders { get; set; }

    public TestImportServiceSettings WebLoader { get; set; }

    public TestImportServiceSettings? BrowserDataLoader { get; set; }

    public bool? Perfomance { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public int ShopId { get; set; }

    public List<TestImportServiceSettings> Services { get; set; } = [];

    IImportServiceSettings IShopImportSettings.WebLoader
    {
        get => WebLoader; set => WebLoader = (TestImportServiceSettings)value;
    }

    IImportServiceSettings IShopImportSettings.ImportService
    {
        get => ImportService; set => ImportService = (TestImportServiceSettings)value;
    }

    IImportServiceSettings? IShopImportSettings.BrowserDataLoader
    {
        get => BrowserDataLoader; set => BrowserDataLoader = (TestImportServiceSettings?)value;
    }

    IImportServiceSettings? IShopImportSettings.RequestHeaders
    {
        get => RequestHeaders; set => RequestHeaders = (TestImportServiceSettings?)value;
    }

    IList IShopImportSettings.Services => Services;

    int? ISettings.ParentSettingsId
    {
        get { return null; }
        set {; }
    }

    protected abstract ShopSettingType ShopSettingType { get; }

    ShopSettingType ISettings.ShopSettingType => ShopSettingType;

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
}
