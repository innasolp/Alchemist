using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures;

public abstract class LoggedContextTestFixture<TWebAppFactory, TEntryPoint> : LoggedContextTest, IClassFixture<TWebAppFactory>, IDisposable
     where TEntryPoint : class
    where TWebAppFactory : WebApplicationFactory<TEntryPoint>, ILoggedContext
{
    protected TWebAppFactory WebAppFactory { get; }

    public LoggedContextTestFixture(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(outputHelper)
    {
        WebAppFactory = webAppFactory;
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    }

    public virtual void Dispose()
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
    }
}
