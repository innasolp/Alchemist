using Alchemist.Messages.Common;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using Xunit.Abstractions;
using ServiceMessage = Import.Service.Commands.ServiceMessage;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceMessageTest(ITestOutputHelper outputHelper) 
 : LoggedContextTest(outputHelper)
{    
    private ImportBackgroundServiceWebAppFactory CreateWebAppFactory(int[] ports)
    {
        if (ports.Length < 6)
            throw new Exception($"No 6 ports in range");
        var webAppFactory = new ImportBackgroundServiceWebAppFactory("serviceMessageTestDb", 
            ports[0], ports[1], ports[2], ports[3], ports[4], ports[5]);

        webAppFactory.FixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;

        return webAppFactory;
    }

    private void ClearWebAppFactory(ImportBackgroundServiceWebAppFactory webAppFactory)
    {
        webAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
        webAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;
    }

    [Fact]
    public async Task SendMessageServiceCreatedSuccess()
    {
        var WebAppFactory = CreateWebAppFactory([8052, 8053, 8202, 8203,8304,8305]);

        var messageReceiver = SignalRHelper.CreateTestSignalRMessageHubReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        var serviceCreatedAutoResetEvent = new AsyncAutoResetEvent();
        var guids = new BlockingCollection<Guid>();
        var semaphoreSlim = new SemaphoreSlim(1, 1);

        Func<ServiceMessage, Task> serviceCreatedAsync = async (serviceMessage) =>
        {
            await OnServiceCreatedAsync(serviceMessage, semaphoreSlim, guids);
            serviceCreatedAutoResetEvent.Set();
        };
        messageReceiver.On(Messages.Common.Messages.ServiceCreated, serviceCreatedAsync);
        await messageReceiver.Start();

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            OutputHelper.WriteLine("Service started.");

            if (guids.Count == 0)
            {
                var waitServiceCreationTask = serviceCreatedAutoResetEvent.WaitAsync();
                await waitServiceCreationTask.WaitAsync(TimeSpan.FromMilliseconds(3000));
                Assert.NotEmpty(guids);
            }
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            ClearWebAppFactory(WebAppFactory);
            await messageReceiver.Stop();            
        }
    }
    private async Task OnServiceCreatedAsync(ServiceMessage serviceMessage, SemaphoreSlim semaphoreSlim, BlockingCollection<Guid> guids,
        CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        guids.TryAdd(serviceMessage.Guid);
        OutputHelper.WriteLine($"Guid {serviceMessage.Guid};Name {serviceMessage.Name}");
        semaphoreSlim.Release();
    }

    [Fact]
    public async Task ReceiveMessageServiceStopSuccess()
    {
        var webAppFactory = CreateWebAppFactory([8054, 8055, 8204, 8205, 8306, 8307]);

        var serviceGuids = new BlockingCollection<Guid>();
        var firstServiceCreatedAutoResetEvent = new AsyncAutoResetEvent(false);
        var serviceStoppedAutoResetEvent = new AsyncAutoResetEvent(false);
        var waitingServiceStoppedTokenSource = new CancellationTokenSource();

        var semaphoreSlim = new SemaphoreSlim(1, 1);
        async Task serviceCreatedAsync(ServiceMessage serviceMessage)
        {
            await OnServiceCreatedAsync(serviceMessage, semaphoreSlim, serviceGuids);
            firstServiceCreatedAutoResetEvent.Set();
        }

        async Task serviceStopAsync(Guid guid)
        {
            serviceStoppedAutoResetEvent.Set();
        }

        var testMessageReceiver = SignalRHelper.CreateTestSignalRMessageHubReceiver(webAppFactory.Services, webAppFactory.SignalRTestServer, "events");
        testMessageReceiver.On(Messages.Common.Messages.ServiceCreated, (Func<ServiceMessage, Task>)serviceCreatedAsync);
        await testMessageReceiver.Start();

        var testMessageSender = SignalRHelper.CreateTestSignalRMessageHubSender(webAppFactory.Services, webAppFactory.SignalRTestServer, "events");

        await testMessageSender.Start();

        try
        {
            var httpClient = webAppFactory.CreateClient();
            OutputHelper.WriteLine("Service started.");
            

            if (serviceGuids.Count == 0)
            {
                var waitServiceCreationTask = firstServiceCreatedAutoResetEvent.WaitAsync();
                await waitServiceCreationTask.WaitAsync(TimeSpan.FromMilliseconds(180000));

                Assert.NotEmpty(serviceGuids);
            }

            var guid = serviceGuids.First();
            testMessageReceiver.On<Guid>(Messages.Common.Messages.ServiceStop, serviceStopAsync);
            await testMessageSender.Send(guid, Messages.Common.Messages.ServiceStop);
            waitingServiceStoppedTokenSource.CancelAfter(20000);
            await serviceStoppedAutoResetEvent.WaitAsync(waitingServiceStoppedTokenSource.Token);

            await Task.Delay(1000);

            var messages = new List<TestLogMessage>(LogMessages);
            Assert.Contains(messages, l => l.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information
            && l.Message?.Contains($"Stopping service {guid} started.") == true);

        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await testMessageReceiver.Stop();
            await testMessageSender.Stop();

            ClearWebAppFactory(webAppFactory);
        }
    }
}
