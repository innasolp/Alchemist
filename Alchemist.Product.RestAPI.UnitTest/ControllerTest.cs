using Alchemist.DataService.Interfaces;
using Message.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.UnitTest;

public abstract class ControllerTest<TController, TEntity>
    where TController : ControllerBase
{
    protected readonly ILogger<TController> _logger = new Logger<TController>(new LoggerFactory());

    protected readonly Mock<IAlchemyRepository> _alchemyRepository = new();

    protected readonly Mock<IMessageSender> _messageSender = new();

    protected readonly ITestOutputHelper _testOutputHelper;

    protected readonly TController Controller;
    protected abstract TController CreateController();

    private readonly Mock<IUrlHelper> _urlHelper = new();

    public ControllerTest(ITestOutputHelper testOutputHelper)
    {
        Controller = CreateController();

        _testOutputHelper = testOutputHelper;

        Controller.Url = _urlHelper.Object;
        _urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Returns(getUrlAction);        

        _messageSender.Setup(m => m.Start(It.IsAny<CancellationToken>())).Returns(StartSender);
        _messageSender.Setup(m => m.Send(It.IsAny<TEntity>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Send<TEntity>);        
    }

    private string getUrlAction(UrlActionContext context)
    {
        return "";
    }

    private async Task StartSender()
    {
        _testOutputHelper.WriteLine("Sender started.");
    }

    private async Task Send<T>(T message, string eventName, CancellationToken cancellationToken = default) { }
}
