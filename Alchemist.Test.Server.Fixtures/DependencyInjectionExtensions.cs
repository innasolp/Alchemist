using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.Server.Fixtures;

public static class DependencyInjectionExtensions
{
    public static void InterceptImplementation<TService, TServiceImplementation>(this IServiceCollection services, TService implementation)
        where TService : class
        where TServiceImplementation : class, TService
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService) &&
         (s.ImplementationType == typeof(TServiceImplementation)
            || s.ImplementationFactory?.Method.ReturnType?.Name == typeof(TServiceImplementation).Name
            || s.ImplementationFactory?.Method.ReturnParameter.ParameterType.Name == typeof(TServiceImplementation).Name
            ));
        descriptors.ToList().ForEach(sd => services.Remove(sd));

        services.AddSingleton<TService>(implementation);
    }

    public static void InterceptImplementation<TService, TServiceImplementation>(this IServiceCollection services, Action<IServiceCollection> implementation)
        where TService : class
        where TServiceImplementation : class, TService
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService) &&
         (s.ImplementationType == typeof(TServiceImplementation)
            || s.ImplementationFactory?.Method.ReturnType?.Name == typeof(TServiceImplementation).Name
            || s.ImplementationFactory?.Method.ReturnParameter.ParameterType.Name == typeof(TServiceImplementation).Name
            ));
        descriptors.ToList().ForEach(sd => services.Remove(sd));

        implementation(services);
    }

    public static void InterceptImplementation<TService>(this IServiceCollection services, TService implementation)
        where TService : class
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService));
        descriptors.ToList().ForEach(sd => services.Remove(sd));

        services.AddSingleton<TService>(implementation);
    }

    public static void RemoveImplementations<TService, TServiceImplementation>(this IServiceCollection services)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService) &&
         (s.ImplementationType == typeof(TServiceImplementation)
            || s.ImplementationFactory?.Method.ReturnType?.Name == typeof(TServiceImplementation).Name
            || s.ImplementationFactory?.Method.ReturnParameter.ParameterType.Name == typeof(TServiceImplementation).Name
            ));
        descriptors.ToList().ForEach(sd => services.Remove(sd));
    }

    public static void RemoveImplementations<TService>(this IServiceCollection services, Type implementationType)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService) &&
         (s.ImplementationType == implementationType
            || s.ImplementationFactory?.Method.ReturnType?.Name == implementationType.Name
            || s.ImplementationFactory?.Method.ReturnParameter.ParameterType.Name == implementationType.Name
            ));
        descriptors.ToList().ForEach(sd => services.Remove(sd));
    }

    public static void RemoveImplementations<TService>(this IServiceCollection services, string implementationTypeString)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService) &&
         (s.ImplementationType?.Name.Contains(implementationTypeString) == true
            || s.ImplementationFactory?.Method.ReturnType?.Name.Contains(implementationTypeString) == true
            || s.ImplementationFactory?.Method.ReturnParameter.ParameterType.Name.Contains(implementationTypeString) == true
            ));
        descriptors.ToList().ForEach(sd => services.Remove(sd));
    }

    public static void RemoveKeyImplementations<TService>(this IServiceCollection services, string implementationTypeString, object key)
    {
        var descriptors = services.Where(s => s.ServiceType == typeof(TService) && s.ServiceKey == key &&
         (s.ImplementationType?.Name.Contains(implementationTypeString) == true
            || s.ImplementationFactory?.Method.ReturnType?.Name.Contains(implementationTypeString) == true
            || s.ImplementationFactory?.Method.ReturnParameter.ParameterType.Name.Contains(implementationTypeString) == true
            ));
        descriptors.ToList().ForEach(sd => services.Remove(sd));
    }
}
