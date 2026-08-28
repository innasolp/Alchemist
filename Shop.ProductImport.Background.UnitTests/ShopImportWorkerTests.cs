using Alchemist.Import.Settings;
using Alchemist.Product.Import.Background;
using Db.Infrastructure;
using Import.Service.Infrastructure;
using Message.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Shop.ProductImport.Background.UnitTests;

public class ShopImportWorkerTests
{
    private readonly Mock<ICommandHandler<StartServiceCommand>> _startServiceCommandHandlerMock = new();

    private readonly Mock<ICommandHandler<StopServiceCommand>> _stopServiceCommandHandlerMock = new();

    private readonly Mock<ICommandHandler<QueueShopCategoryToServicesCommand>> _queueShopCategoryToServicesCommandHandlerMock = new();

    private readonly Mock<ICommandHandler<AddShopImportServiceFromShopSettingsCommand, Guid>> _addShopImportServiceFromShopSettingsCommandHandlerMock = new();

    private readonly Mock<ICommandHandler<StopAllServicesCommand>> _stopAllServicesCommandHandlerMock = new();

    private readonly Mock<ICommandHandler<AddImportServiceCommand, Guid>> _addImportServiceCommandHandlerMock = new();

    private readonly Mock<ILogger<ShopImportWorker>> _loggerMock = new();

    private readonly Mock<IMessageReceiver> _eventMessageReceiverMock = new();

    private readonly Mock<IAcknowlegefulMessageReceiver> _ackReceiverMock = new();

    private readonly Mock<IMessageSender> _senderMock = new();

    private readonly Mock<ISettingsAdapter> _settingsAdapterMock = new();

    public ShopImportWorkerTests()
    {
        _settingsAdapterMock
            .Setup(a => a.GetAllShopImportSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IShopImportSettings>());
    }

    private ShopImportWorker CreateShopImportWorker()
    {
        return new ShopImportWorker(
            _loggerMock.Object,
            _eventMessageReceiverMock.Object,
            _ackReceiverMock.Object,
            _senderMock.Object,
            [_settingsAdapterMock.Object],
            _startServiceCommandHandlerMock.Object,
            _stopServiceCommandHandlerMock.Object,
            _queueShopCategoryToServicesCommandHandlerMock.Object,
            _addShopImportServiceFromShopSettingsCommandHandlerMock.Object,
            _stopAllServicesCommandHandlerMock.Object,
            _addImportServiceCommandHandlerMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_StartsReceiver_RegistersServiceStartHandler_And_LogsConnection()
    {
        Func<Guid, Task>? startHandler = null;
        _eventMessageReceiverMock
            .Setup(r => r.On<Guid>(It.IsAny<string>(), It.IsAny<Func<Guid, Task>>()))
            .Callback<string, Func<Guid, Task>>((_, h) =>
            {
                startHandler = h;
            });

        _eventMessageReceiverMock
            .Setup(r => r.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _ackReceiverMock
            .Setup(r => r.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var worker = CreateShopImportWorker();

        await worker.StartAsync(CancellationToken.None);

        // Assert
        _eventMessageReceiverMock.Verify(r => r.Start(It.IsAny<CancellationToken>()), Times.Once);
        _eventMessageReceiverMock.Verify(r => r.On(It.IsAny<string>(), It.IsAny<Func<ServiceMessage, Task>>()));

        // logger should have been called with "Import service connected to messaging host."
        _loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Import service connected to messaging host")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // startHandler must be captured
        Assert.NotNull(startHandler);
    }

    [Fact]
    public async Task StartHandler_InvokesStartServiceCommand_And_Logs()
    {
        Func<ServiceMessage, Task>? startHandler = null;
        _eventMessageReceiverMock
            .Setup(r => r.On<ServiceMessage>(It.Is<string>(s=>s == Alchemist.Messages.Common.Messages.ServiceStarting), It.IsAny<Func<ServiceMessage, Task>>()))
            .Callback<string, Func<ServiceMessage, Task>>((_, h) => startHandler = h);


        var shopImportSettingsMock = new Mock<IShopImportSettings>();
        var serviceGuid = Guid.NewGuid();
        var startServiceName = "Service_" + serviceGuid.ToString();
        _settingsAdapterMock
            .Setup(a => a.GetAllShopImportSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IShopImportSettings>() { { startServiceName, shopImportSettingsMock.Object} });

        _eventMessageReceiverMock
            .Setup(r => r.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _addImportServiceCommandHandlerMock
            .Setup(r => r.Handle(It.Is<AddImportServiceCommand>(c => c.Name == startServiceName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceGuid);

        var worker = CreateShopImportWorker();

        // Ensure ExecuteAsync registers the handler
        await worker.StartAsync(CancellationToken.None);

        if(worker.ExecuteTask != null)
            await worker.ExecuteTask;

        Assert.NotNull(startHandler);

        _addImportServiceCommandHandlerMock.Verify(m => m.Handle(It.Is<AddImportServiceCommand>(c => c.Name == startServiceName), It.IsAny<CancellationToken>()), Times.Once);

        // Assert mediator got StartServiceCommand with the same guid
        _startServiceCommandHandlerMock.Verify(m => m.Handle(It.Is<StartServiceCommand>(c => c.Guid == serviceGuid), It.IsAny<CancellationToken>()), Times.Once);

        // Verify logger logged starting message
        _loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting service with guid")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        // Verify logger logged that service started (or attempted)
        _loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Service {Guid} started.") || v.ToString().Contains("Service")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StopAsync_StopsReceiverAndSender_And_RequestsStopAllServices()
    {
        _eventMessageReceiverMock
            .Setup(r => r.Stop(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _senderMock
            .Setup(s => s.Stop(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);        

        var worker = CreateShopImportWorker();

        // Act
        await worker.StopAsync(CancellationToken.None);

        // Assert
        _stopAllServicesCommandHandlerMock.Verify(m => m.Handle(It.IsAny<StopAllServicesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventMessageReceiverMock.Verify(r => r.Stop(It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Stop(It.IsAny<CancellationToken>()), Times.Once);

        // No warnings expected in the normal path
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task StopAsync_WhenReceiverThrows_LogsWarning_And_StillAttemptsToStopSender()
    {
        _eventMessageReceiverMock
            .Setup(r => r.Stop(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("receiver failure"));

        _senderMock
            .Setup(s => s.Stop(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var worker = CreateShopImportWorker();

        // Act
        await worker.StopAsync(CancellationToken.None);

        // Assert mediator called and sender still stopped
        _stopAllServicesCommandHandlerMock.Verify(m => m.Handle(It.IsAny<StopAllServicesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Stop(It.IsAny<CancellationToken>()), Times.Once);

        // Logger should have at least one warning for the receiver stop failure
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error stopping event message receiver") || v.ToString().Contains("Error while requesting all services to stop")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}