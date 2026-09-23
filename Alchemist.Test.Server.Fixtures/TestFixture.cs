using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestFixture<TWebAppFactory, TEntryPoint>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : IClassFixture<TWebAppFactory>
     where TEntryPoint : class    
    where TWebAppFactory : WebApplicationFactory<TEntryPoint>
{
    protected TWebAppFactory WebAppFactory { get; private set; } = webAppFactory;    

    protected ITestOutputHelper OutputHelper { get; private set; } = outputHelper;    
}