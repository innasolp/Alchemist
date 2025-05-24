using Alchemist.Import.Product.Test.Infrastructure;
using Moq;

namespace Alchemist.Import.Product.Test;

public class ImportShopProductServiceTest:ImportProductsTest
{
    [Fact]
    public async Task ImportWasStoppedWhenWebLoaderNotExecuted()
    {
        var exception = new InvalidOperationException("test fatal error");
        var name = Guid.NewGuid().ToString();
        Service.SetName(name);

        WebLoaderMock.Reset();
        WebLoaderMock.Setup(w => w.Start()).Throws(exception);

        var token = new CancellationTokenSource();
        await Service.Start(token.Token);

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("ServiceWasStopped"), name);

        LoggerMock.VerifyError(exception, ServiceResourceManager.GetString("ImportWasStoppedWebLoaderNotExecute"), WebLoaderMock.Object.GetType().Name); 
    }

    [Fact]
    public async Task ImportStartedWhenWebLoaderExecutedSuccessfull()
    {
        var name = Guid.NewGuid().ToString();
        Service.SetName(name);

        WebLoaderMock.Reset();
        WebLoaderMock.SetupStartSuccess();

        var token = new CancellationTokenSource();

        var task = Service.StartServiceInFactoryAsync(token.Token);        
        
        await Task.Delay(2000);

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("ServiceStarted"), name);        
    }

    [Fact]
    public async Task ImportStoppedWhenCancellationRequested()
    {
        var name = Guid.NewGuid().ToString();
        Service.SetName(name);

        WebLoaderMock.Reset();
        WebLoaderMock.SetupStartSuccess();
       
        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(1000);

        await token.CancelAsync();

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("ServiceWasStopped"), name);
    }    
}