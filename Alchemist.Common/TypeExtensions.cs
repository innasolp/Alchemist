using System.Reflection;

namespace Alchemist.Common;

public static class TypeExtensions
{
    public static Type[] GetImplmentationsForInterface(this Type interfaceType, string path)
    {
        if (!Directory.Exists(path))
            throw new Exception($"folder for {interfaceType}  does not exists");
        var assembliesDir = Directory.GetDirectories(path);

        List<Assembly> assemblies = new List<Assembly>();
        if (assembliesDir.Length != 0)
            assemblies = assembliesDir.SelectMany(d => Directory.GetFiles(d, "*.dll")).Select(Assembly.LoadFrom).ToList();
        if (assemblies.Count == 0)
            assemblies = Directory.GetFiles(path, "*.dll").Select(Assembly.LoadFrom).ToList();
        return assemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(interfaceType)).ToArray();
    }

    public static Dictionary<Assembly,Type[]> GetAssemblyImplmentationsForInterface(this Type interfaceType, string path)
    {
        if (!Directory.Exists(path))
            throw new Exception($"folder for {interfaceType}  does not exists");
        var assembliesDir = Directory.GetDirectories(path);

        List<Assembly> assemblies = [];

        if (assembliesDir.Length != 0)
            assemblies = assembliesDir.SelectMany(d => Directory.GetFiles(d, "*.dll")).Select(Assembly.LoadFrom).ToList();
        if (assemblies.Count == 0)
            assemblies = Directory.GetFiles(path, "*.dll").Select(Assembly.LoadFrom).ToList();

       return assemblies.Select(a => new { assembly = a, types = a.GetTypes().Where(t => t.IsImplementation(interfaceType)) })
            .ToDictionary(at => at.assembly, at => at.types.ToArray());          
    }

    public static bool IsImplementation(this Type type, Type interfaceType)
    {
        return type.IsClass && !type.IsAbstract && type.GetInterfaces().Contains(interfaceType);
    }

    public static Type GetImplementationType(this Type interfaceType, string path, string className)
    {
        var types = interfaceType.GetImplmentationsForInterface(path);
        if (types.Length == 0)
            throw new Exception($"No implementations for {interfaceType.Name} by '{className}' in {path}");
        if (types.Length > 1)
            throw new Exception($"Too many implementations for {interfaceType.Name} by '{className}' in {path}");
        return types[0];
    }

    public static string GetNameWithoutGenericArity(this Type t)
    {
        string name = t.Name;
        int index = name.IndexOf('`');
        return index == -1 ? name : name[..index];
    }
}
