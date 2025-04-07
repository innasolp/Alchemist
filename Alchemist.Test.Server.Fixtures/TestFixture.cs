using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestFixture<TWebAppFactory, TEntryPoint, TDbContext>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : IClassFixture<TWebAppFactory>, IDisposable
     where TEntryPoint : class
    where TDbContext : DbContext
    where TWebAppFactory : AlchemistWebAppFactory<TEntryPoint, TDbContext>
{
    protected TWebAppFactory WebAppFactory { get; private set; } = webAppFactory;

    protected HttpClient HttpClient { get; private set; } = webAppFactory.CreateClient();

    protected ITestOutputHelper OutputHelper { get; private set; } = outputHelper;

    public virtual void Dispose()
    {
        HttpClient.Dispose();
    }
}
