using Microsoft.EntityFrameworkCore;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestFixture<TWebAppFactory, TEntryPoint, TDbContext>(TWebAppFactory webAppFactory) : IClassFixture<TWebAppFactory>
     where TEntryPoint : class
    where TDbContext : DbContext
    where TWebAppFactory : AlchemistWebAppFactory<TEntryPoint, TDbContext>
{
    protected TWebAppFactory WebAppFactory { get; private set; } = webAppFactory;

    protected HttpClient HttpClient { get; private set; } = webAppFactory.CreateClient();
}
