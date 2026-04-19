using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.Server.Fixtures;

public interface IWebHostBuilderConfigure
{
    event Action<WebHostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration;
    

    event Action<WebHostBuilderContext, IServiceCollection> ConfigureWebHostBuilderContextServices;
}