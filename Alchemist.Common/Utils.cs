using System.Reflection;

namespace Alchemist.Common;

public static class Utils
{
    private static Dictionary<PlatformID, string> _separators = new()
    {
        {PlatformID.Win32NT,"\\" },
        {PlatformID.Unix, "/" }
    };

    public static string CombinePath(params string[] pathes)
    {
        var sep = _separators[Environment.OSVersion.Platform];
        var result = "";
        foreach (var path in pathes)
        {
            var nonValidSeparators = _separators.Where(s => path.Contains(s.Value) && s.Key != Environment.OSVersion.Platform).ToList();
            var currentPath = path;
            nonValidSeparators.ForEach(s => currentPath = currentPath.Replace(s.Value, sep));
            result += (result.Length > 0 ? sep : "") + currentPath;
        }
        return result;
    }

    public static bool IsInDocker()
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }

    private const string LocalHostVariable = "$[localhost]";
    private const string OSlocalHost = "localhost";
    private const string DockerLocalhost = "host.docker.internal";

    public static bool ContainsLocalHost(this string value)
    {
        return value.Contains(LocalHostVariable);
    }

    public static string SetEnvironmentLocalHostIfNeed(this string value)
    {
        if (!value.Contains(LocalHostVariable)) return value;
        return value.Replace(LocalHostVariable, GetEnvironmentLocalhost()); 
    }

    public static string GetEnvironmentLocalhost()
    {
        return IsInDocker() ? DockerLocalhost : OSlocalHost;
    }

    public static string GetAppPath()
    {
        if( Environment.OSVersion.Platform == PlatformID.Unix) return "/src";// : "..//..//..//..//"; 
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) == Directory.GetCurrentDirectory()
            ? "..//..//..//..//" : "..//";
    }
}
