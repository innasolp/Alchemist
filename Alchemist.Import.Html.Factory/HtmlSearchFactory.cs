using DependencyInjection.ImplementationFactory;
using Microsoft.Extensions.DependencyInjection; 

namespace Alchemist.Import.Html.Factory;

public class HtmlSearchFactory : IServiceImplementationFactory, IServiceImplementationFactory<IHtmlSearcher>
{
    public static IHtmlSearcher CreateSearcher(SearchMatchType searchMatchType, SearchElementType searchElementType = SearchElementType.Attribute)
    {
        if (searchElementType == SearchElementType.Attribute)
            switch (searchMatchType)
            {
                case SearchMatchType.Equals:
                    return new EqualsAttributeHtmlSearcher();

                case SearchMatchType.Like:
                    return new LikeAttributeHtmlSearcher();

                default:
                    return new EmptySearcher();
            }
        else
            switch (searchMatchType)
            {
                case SearchMatchType.Equals:
                    return new EqualsByJsonValueHtmlSearcher();

                case SearchMatchType.Like:
                    return new LikeAttributeHtmlSearcher();

                default:
                    return new EmptySearcher();
            }
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType, object? key)
    {
        var options = serviceProvider.GetRequiredKeyedService<HtmlSearchFactoryOptions>(key);
        return CreateSearcher(options.SearchMatchType, options.SearchElementType);
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType)
    {
        var options = serviceProvider.GetRequiredService<HtmlSearchFactoryOptions>();
        return CreateSearcher(options.SearchMatchType, options.SearchElementType);
    }

    IHtmlSearcher IServiceImplementationFactory<IHtmlSearcher>.GetService(IServiceProvider serviceProvider, object? key)
    {
        var options = serviceProvider.GetRequiredKeyedService<HtmlSearchFactoryOptions>(key);
        return CreateSearcher(options.SearchMatchType, options.SearchElementType);
    }

    IHtmlSearcher IServiceImplementationFactory<IHtmlSearcher>.GetService(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<HtmlSearchFactoryOptions>();
        return CreateSearcher(options.SearchMatchType, options.SearchElementType);
    }
}
