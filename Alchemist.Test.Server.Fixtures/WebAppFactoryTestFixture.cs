using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Alchemist.Test.Server.Fixtures;

public class WebAppFactoryTestFixture<TWebAppFactory, TEntryPoint> : IClassFixture<TWebAppFactory>
    where TEntryPoint : Program
    where TWebAppFactory : WebApplicationFactory<TEntryPoint>
{
}
