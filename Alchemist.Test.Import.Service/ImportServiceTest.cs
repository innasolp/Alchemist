using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using WebLoader.Common;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Test.Import.Service;

public abstract class ImportServiceTest<TService, TLogger>
    where TService:ShopImportService
    where TLogger:class, ILogger
{
    protected readonly ITestOutputHelper _outputHelper;

    protected ResourceManager ServiceResourceManager { get; }
   
    protected Mock<TLogger> LoggerMock { get; } = new Mock<TLogger>();

    protected Mock<IWebLoader> WebLoaderMock { get; } = new Mock<IWebLoader>();    

    protected RequestHeaders RequestHeaders { get; } = new RequestHeaders();    

    protected abstract TService Service { get; }        

    protected ImportServiceTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;

        ServiceResourceManager = new ResourceManager("Alchemist.Import.Service.LogMessages",
                               typeof(ShopImportService).Assembly);
    }
}
