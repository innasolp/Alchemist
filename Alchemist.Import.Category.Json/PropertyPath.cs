namespace Alchemist.Import.Category.Json;

public class PropertyPath(string propertyName, string path, bool? loadStopIfNotExists = false)
{
    public string PropertyName { get; set; } = propertyName;
    public string Path { get; set; } = path;
    public bool? LoadStopIfNotExists { get; set; } = loadStopIfNotExists;
}
