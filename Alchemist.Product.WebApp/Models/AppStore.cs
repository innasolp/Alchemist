using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.WebApp.Models;

internal class AppStore(Controller controller)
{
    private readonly Controller _controller = controller;

    public const string ShopApp = "shopapp";
    public const string ImportSettingsApp = "importsettingsapp";

    private static readonly string[] _availableAppKeys = [ShopApp, ImportSettingsApp];

    private static readonly Dictionary<string, string> _defaultAppUrls = new()
    {
        {ShopApp, "/Shop"},
        {ImportSettingsApp, "/Import/Settings"}
    };

    public void SetAppUrl(string appKey, string url)
    {
        _controller.HttpContext.Session.SetString(appKey, url);        
    }

    public bool TryGetAppUrl(string appKey, out string? url)
    {
        url = _controller.HttpContext.Session.GetString(appKey);
        return !string.IsNullOrEmpty(url);
    }

    public Dictionary<string, string> GetAppUrls()
    {
        var appUrls = new Dictionary<string, string>();
        foreach(var appKey in _availableAppKeys)
        {
            if (TryGetAppUrl(appKey, out string? appUrl) && appUrl != null)
                appUrls.Add(appKey, appUrl);
            else
                appUrls.Add(appKey, _defaultAppUrls[appKey]);
        }
        return appUrls;
    }
}
