using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public interface IWebHostConfigure : IWebHostBuilderConfigure
{
    event Action<IHost> ConfigureHost;
}