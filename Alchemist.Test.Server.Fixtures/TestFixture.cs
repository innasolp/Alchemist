using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestFixture<TWebAppFactory, TEntryPoint>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : IClassFixture<TWebAppFactory>, IDisposable
     where TEntryPoint : class    
    where TWebAppFactory : WebApplicationFactory<TEntryPoint>
{
    protected TWebAppFactory WebAppFactory { get; private set; } = webAppFactory;

    protected HttpClient HttpClient { get; private set; } = webAppFactory.CreateClient();

    protected ITestOutputHelper OutputHelper { get; private set; } = outputHelper;

    public virtual void Dispose()
    {
        HttpClient.Dispose();
    }
}
