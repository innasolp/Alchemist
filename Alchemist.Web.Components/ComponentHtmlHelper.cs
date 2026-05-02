using System.Reflection;


namespace Alchemist.Web.Components;

internal static class ComponentHtmlHelper
{
    internal static string GetHtml(string embeddedFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Get the manifest resource names (useful for debugging and finding the correct name)
        //string[] resourceNames = assembly.GetManifestResourceNames();       

        using var stream = assembly.GetManifestResourceStream(embeddedFileName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return content;
        }
        else
        {
            throw new InvalidDataException($"Resource '{embeddedFileName}' not found.");
        }
    }
}
