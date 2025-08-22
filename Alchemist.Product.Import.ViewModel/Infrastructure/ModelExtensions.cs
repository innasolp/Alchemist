using Alchemist.Import.Settings.Interfaces;


namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelExtensions
{
   public static bool FieldsEquals(this IImportServiceSettings source, IImportServiceSettings target)
    {
        return source.Name == target.Name
           && ((string.IsNullOrEmpty(source.ServiceTypeName) && string.IsNullOrEmpty(target.ServiceTypeName))
             || string.Equals(source.ServiceTypeName, target.ServiceTypeName, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(source.AssemblyPath) && string.IsNullOrEmpty(target.AssemblyPath))
             || string.Equals(source.AssemblyPath, target.AssemblyPath, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(source.ServiceProviderPath) && string.IsNullOrEmpty(target.ServiceProviderPath))
             || string.Equals(source.ServiceProviderPath, target.ServiceProviderPath, StringComparison.CurrentCultureIgnoreCase))
             && ((string.IsNullOrEmpty(source.ImplementationTypeName) && string.IsNullOrEmpty(target.ImplementationTypeName))
             || string.Equals(source.ImplementationTypeName, target.ImplementationTypeName, StringComparison.CurrentCultureIgnoreCase));
    }
}
