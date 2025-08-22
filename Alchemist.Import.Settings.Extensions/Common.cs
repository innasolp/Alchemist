namespace Alchemist.Import.Settings.Extensions;

public static class Common
{
    public static string[] GetPrimaryServiceNames()
    {
        return Enum.GetNames<PrimaryServiceName>();
    }
}
