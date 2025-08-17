using Alchemist.Import.Service;
using BrowserDataLoader.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using WebLoader.Common;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Test.Import.Service;

public abstract class ImportServiceTest<TService, TLogger>(ITestOutputHelper outputHelper)
    where TService:ShopImportService
    where TLogger:class, ILogger
{
    protected readonly ITestOutputHelper _outputHelper = outputHelper;

    protected ResourceManager ServiceResourceManager { get; } = new ResourceManager("Alchemist.Import.Service.LogMessages",
                               typeof(ShopImportService).Assembly);

    protected Mock<TLogger> LoggerMock { get; } = new Mock<TLogger>();

    protected Mock<IWebLoader> WebLoaderMock { get; } = new Mock<IWebLoader>(); 
    
    protected Mock<IBrowserDataLoader> BrowserDataLoaderMock { get; } = new Mock<IBrowserDataLoader>();    

    protected RequestHeaders RequestHeaders { get; } = new RequestHeaders();

    protected List<WebLoader.Interfaces.ICookieData> Cookies { get; } = [];

    protected abstract TService Service { get; }
}
