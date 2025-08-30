using Alchemist.Import.Service;
using Alchemist.Test.Import.Service.Infrastructure;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Alchemist.Test.Import.Service;

public abstract class ImportServiceExecutionTest<TService, TLogger>(ITestOutputHelper outputHelper) 
    : ImportServiceTest<TService, TLogger>(outputHelper)
    where TService : ShopImportService, ITestService
    where TLogger : class, ILogger
{
    protected async Task ImportWasStoppedWhenWebLoaderNotExecutedAsync()
    {
        var exception = new InvalidOperationException("test fatal error");
        var name = Guid.NewGuid().ToString();
        Service.SetName(name);

        LoaderMock.Setup(l => l.Name).Returns(Guid.NewGuid().ToString());
        LoaderMock.Setup(w => w.Start()).Throws(exception);

        var token = new CancellationTokenSource();
        await Service.Start(token.Token);

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("ServiceWasStopped"), name);

        LoggerMock.VerifyError(exception, ServiceResourceManager.GetString("ImportWasStoppedWebLoaderNotExecute"), LoaderMock.Object.Name);
    }

    protected async Task ImportStartedWhenWebLoaderExecutedSuccessfullAsync()
    {
        var name = Guid.NewGuid().ToString();
        Service.SetName(name);

        LoaderMock.SetupStartSuccess();

        var token = new CancellationTokenSource();

        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(2000);

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("ServiceStarted"), name);
    }

    protected async Task ImportStoppedWhenCancellationRequestedAsync()
    {
        var name = Guid.NewGuid().ToString();
        Service.SetName(name);

        LoaderMock.SetupStartSuccess();

        var token = new CancellationTokenSource();      

        var task = Service.Start(token.Token);

        await Task.Delay(500);        

        await token.CancelAsync();
     
        await Task.Delay(1000);        

        await task.WaitAsync(token.Token); 

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("ServiceWasStopped"), name);
    }
}
