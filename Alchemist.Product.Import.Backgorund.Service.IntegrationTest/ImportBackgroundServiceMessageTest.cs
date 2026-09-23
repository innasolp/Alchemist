using Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Import.Service.Infrastructure;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using Test.DbContainer.Abstractions;
using Test.PostresqlTestContainer;
using Test.RedisTestContainer;
using Xunit.Abstractions;
using ServiceMessage = Import.Service.Infrastructure.ServiceMessage;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

[Collection(nameof(PostgresRedisDbCollection))]
public class ImportBackgroundServiceMessageTest(ITestOutputHelper outputHelper,
    DbTestContainerFixture<PostgresqlTestDbContainer> postgresFixture,
    DbTestContainerFixture<RedisTestDbContainer> redisFixture) 
 : LoggedContextTest(outputHelper)
{
    private readonly DbTestContainerFixture<PostgresqlTestDbContainer> _postgresFixture = postgresFixture;
    private readonly DbTestContainerFixture<RedisTestDbContainer> _redisFixture = redisFixture;

    private async Task<ImportBackgroundServiceWebAppFactory> CreateWebAppFactoryAsync(int redisIndex, string childjobstorage, int[] ports)
    {
        if (ports.Length < 6)
            throw new Exception($"No 6 ports in range");
        var webAppFactory = new ImportBackgroundServiceWebAppFactory(_postgresFixture.Container, _redisFixture.Container, "serviceMessageTestDb", 
             ports[0], ports[1], ports[2], ports[3], ports[4], ports[5], redisIndex, childjobstorage);

        await webAppFactory.InitializeAsync();

        webAppFactory.FixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;

        return webAppFactory;
    }

    private async Task ClearWebAppFactoryAsync(ImportBackgroundServiceWebAppFactory webAppFactory)
    {
        webAppFactory.FixtureLoggingContext.LoggedMessage -= Log;
        webAppFactory.ShopApiFixtureLoggingContext.LoggedMessage += Log;
        webAppFactory.SettingsApiFixtureLoggingContext.LoggedMessage += Log;

        await (webAppFactory as IAsyncLifetime).DisposeAsync();
    }

    [Fact]
    public async Task SendMessageServiceCreatedSuccess()
    {
        await using var WebAppFactory = await CreateWebAppFactoryAsync(2, "childjobstorage_created", [8052, 8053, 8202, 8203,8304,8305]);

        var messageReceiver = SignalRHelper.CreateTestSignalRMessageHubReceiver(WebAppFactory.Services, WebAppFactory.SignalRTestServer, "events");
        var serviceCreatedAutoResetEvent = new AsyncAutoResetEvent();
        var guids = new BlockingCollection<Guid>();
        var semaphoreSlim = new SemaphoreSlim(1, 1);

        Func<ServiceMessage, Task> serviceCreatedAsync = async (serviceMessage) =>
        {
            await OnServiceAsync(serviceMessage, semaphoreSlim, guids);
            serviceCreatedAutoResetEvent.Set();
        };
        messageReceiver.On(Messages.Common.Messages.ServiceCreated, serviceCreatedAsync);
        await messageReceiver.Start();

        try
        {
            OutputHelper.WriteLine("Service started.");

            if (guids.Count == 0)
            {
                var waitServiceCreationTask = serviceCreatedAutoResetEvent.WaitAsync();
                await waitServiceCreationTask.WaitAsync(TimeSpan.FromMilliseconds(60000));
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
            await ClearWebAppFactoryAsync(WebAppFactory);
            await messageReceiver.Stop();            
        }
    }
    private async Task OnServiceAsync(ServiceMessage serviceMessage, SemaphoreSlim semaphoreSlim, BlockingCollection<Guid> guids,
        CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        guids.TryAdd(serviceMessage.Guid);
        OutputHelper.WriteLine($"Guid {serviceMessage.Guid};Name {serviceMessage.Name}");
        semaphoreSlim.Release();
    }
    
    [Fact]
    public async Task SendMessageServiceStartedSuccess()
    {
        await using var webAppFactory = await CreateWebAppFactoryAsync(4, "childjobstorage_start",[8056, 8057, 8206, 8207, 8308, 8309]);

        var messageReceiver = SignalRHelper.CreateTestSignalRMessageHubReceiver(webAppFactory.Services, webAppFactory.SignalRTestServer, "events");
        var serviceStartedAutoResetEvent = new AsyncAutoResetEvent();
        var guids = new BlockingCollection<Guid>();
        var semaphoreSlim = new SemaphoreSlim(1, 1);

        Func<ServiceStartedMessage, Task> serviceStartedAsync = async (serviceMessage) =>
        {
            await OnServiceAsync(serviceMessage, semaphoreSlim, guids);
            serviceStartedAutoResetEvent.Set();
        };
        messageReceiver.On(Messages.Common.Messages.ServiceStarted, serviceStartedAsync);
        await messageReceiver.Start();

        try
        {
            var httpClient = webAppFactory.CreateClient();
            
            if (guids.Count == 0)
            {
                var waitServiceStartingTask = serviceStartedAutoResetEvent.WaitAsync();
                await waitServiceStartingTask.WaitAsync(TimeSpan.FromMilliseconds(120000));
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
            await ClearWebAppFactoryAsync(webAppFactory);
            await messageReceiver.Stop();
        }
    }

    [Fact]
    public async Task ReceiveMessageServiceStopSuccess()
    {
        await using var webAppFactory = await CreateWebAppFactoryAsync(3, "childjobstorage_stop",[8054, 8055, 8204, 8205, 8306, 8307]);

        var serviceGuids = new BlockingCollection<Guid>();
        var firstServiceCreatedAutoResetEvent = new AsyncAutoResetEvent(false);
        var serviceStoppedAutoResetEvent = new AsyncAutoResetEvent(false);
        var waitingServiceStoppedTokenSource = new CancellationTokenSource();

        var semaphoreSlim = new SemaphoreSlim(1, 1);
        async Task serviceCreatedAsync(ServiceMessage serviceMessage)
        {
            await OnServiceAsync(serviceMessage, semaphoreSlim, serviceGuids);
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

            await ClearWebAppFactoryAsync(webAppFactory);
        }
    }    
}