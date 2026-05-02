using Alchemist.Import.Settings;
using Alchemist.Product.Import.Background;
using Import.Service.Infrastructure;
using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Shop.ProductImport.Background.UnitTests;

public class ShopImportWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_StartsReceiver_RegistersServiceStartHandler_And_LogsConnection()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShopImportWorker>>();
        var mediatorMock = new Mock<IMediator>();

        var receiverMock = new Mock<IMessageReceiver>();
        var senderMock = new Mock<IMessageSender>();

        // Capture the registered start handler (registered in StartEventMessageReceiverAsync)
        Func<Guid, Task>? startHandler = null;
        receiverMock
            .Setup(r => r.On<Guid>(It.IsAny<string>(), It.IsAny<Func<Guid, Task>>()))
            .Callback<string, Func<Guid, Task>>((_, h) =>
            {
                startHandler = h;
            });

        receiverMock
            .Setup(r => r.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // adapters returning empty dictionary to keep ExecuteAsync quick
        var adapterMock = new Mock<ISettingsAdapter>();
        adapterMock
            .Setup(a => a.GetAllShopImportSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IShopImportSettings>());

        var worker = new ShopImportWorker(
            loggerMock.Object,
            mediatorMock.Object,
            receiverMock.Object,
            senderMock.Object,
            [adapterMock.Object]
        );

        // Act
        await worker.StartAsync(CancellationToken.None);

        // Assert
        receiverMock.Verify(r => r.Start(It.IsAny<CancellationToken>()), Times.Once);
        receiverMock.Verify(r => r.On(It.IsAny<string>(), It.IsAny<Func<ServiceMessage, Task>>()));

        // logger should have been called with "Import service connected to messaging host."
        loggerMock.Verify(l => l.Log(
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
    public async Task StartHandler_InvokesMediator_StartsService_And_Logs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ShopImportWorker>>();
        var mediatorMock = new Mock<IMediator>();

        var receiverMock = new Mock<IMessageReceiver>();
        var senderMock = new Mock<IMessageSender>();

        Func<Guid, Task>? startHandler = null;
        receiverMock
            .Setup(r => r.On<Guid>(It.IsAny<string>(), It.IsAny<Func<Guid, Task>>()))
            .Callback<string, Func<Guid, Task>>((_, h) => startHandler = h);

        receiverMock
            .Setup(r => r.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var adapterMock = new Mock<ISettingsAdapter>();
        adapterMock
            .Setup(a => a.GetAllShopImportSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IShopImportSettings>());

        var worker = new ShopImportWorker(
            loggerMock.Object,
            mediatorMock.Object,
            receiverMock.Object,
            senderMock.Object,
            new[] { adapterMock.Object }
        );

        // Ensure ExecuteAsync registers the handler
        await worker.StartAsync(CancellationToken.None);
        Assert.NotNull(startHandler);

        var serviceGuid = Guid.NewGuid();

        // Act - invoke handler as if message came from message bus
        await startHandler!(serviceGuid);

        // Assert mediator got StartServiceCommand with the same guid
        mediatorMock.Verify(m => m.Send(It.Is<StartServiceCommand>(c => c.Guid == serviceGuid), It.IsAny<CancellationToken>()), Times.Once);

        // Verify logger logged starting message
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting service with guid")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        // Verify logger logged that service started (or attempted)
        loggerMock.Verify(l => l.Log(
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
        // Arrange
        var loggerMock = new Mock<ILogger<ShopImportWorker>>();
        var mediatorMock = new Mock<IMediator>();

        var receiverMock = new Mock<IMessageReceiver>();
        receiverMock
            .Setup(r => r.Stop(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var senderMock = new Mock<IMessageSender>();
        senderMock
            .Setup(s => s.Stop(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var adapterMock = new Mock<ISettingsAdapter>();
        adapterMock
            .Setup(a => a.GetAllShopImportSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IShopImportSettings>());

        var worker = new ShopImportWorker(
            loggerMock.Object,
            mediatorMock.Object,
            receiverMock.Object,
            senderMock.Object,
            new[] { adapterMock.Object }
        );

        // Act
        await worker.StopAsync(CancellationToken.None);

        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<StopAllServicesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        receiverMock.Verify(r => r.Stop(It.IsAny<CancellationToken>()), Times.Once);
        senderMock.Verify(s => s.Stop(It.IsAny<CancellationToken>()), Times.Once);

        // No warnings expected in the normal path
        loggerMock.Verify(l => l.Log(
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
        // Arrange
        var loggerMock = new Mock<ILogger<ShopImportWorker>>();
        var mediatorMock = new Mock<IMediator>();

        var receiverMock = new Mock<IMessageReceiver>();
        receiverMock
            .Setup(r => r.Stop(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("receiver failure"));

        var senderMock = new Mock<IMessageSender>();
        senderMock
            .Setup(s => s.Stop(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var adapterMock = new Mock<ISettingsAdapter>();
        adapterMock
            .Setup(a => a.GetAllShopImportSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IShopImportSettings>());

        var worker = new ShopImportWorker(
            loggerMock.Object,
            mediatorMock.Object,
            receiverMock.Object,
            senderMock.Object,
            [adapterMock.Object]
        );

        // Act
        await worker.StopAsync(CancellationToken.None);

        // Assert mediator called and sender still stopped
        mediatorMock.Verify(m => m.Send(It.IsAny<StopAllServicesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        senderMock.Verify(s => s.Stop(It.IsAny<CancellationToken>()), Times.Once);

        // Logger should have at least one warning for the receiver stop failure
        loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error stopping event message receiver") || v.ToString().Contains("Error while requesting all services to stop")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}