using System.ComponentModel;

namespace Alchemist.Import.Settings.Extensions;

public enum PrimaryServiceName
{
    [Description("ImportService")]
    ImportService = 0,

    [Description("BrowserDataLoader")]
    BrowserDataLoader = 1,

    [Description("BrowserLauncher")]
    BrowserLauncher = 2,

    [Description("RequestHeaders")]
    RequestHeaders = 3,

    [Description("WebLoader")]
    WebLoader = 4
}
