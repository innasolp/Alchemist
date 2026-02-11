using Alchemist.Product.Data;
using Alchemist.Settings.RestAPI.Controllers;
using Mediator.Infrastructure.Command;
using Mediator.Infrastructure.Events;
using Mediator.Infrastructure.Request;
using MediatR;
using Message.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using ShopSettings.Infrastructure;
using ShopSettings.UnitOfWork;
using UnitOfWork;
using static Alchemist.Settings.RestAPI.Controllers.SettingsController;

namespace ShopSettings.API.UnitTest;

public class SettingsControllerUnitTests
{
    private readonly ILogger<SettingsController> _logger
        = new Logger<SettingsController>(new LoggerFactory());

    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMessageSender> _messageSender = new();
    private readonly Mock<IShopSettingsRepository> _shopSettingsRepository = new();
    private readonly Mock<IRepository<Alchemist.Product.Data.ShopSettings>> _repo = new();

    private readonly SettingsController _controller;

    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<IUnitOfWork<IDbContextTransaction>> _unitOfWorkMock = new();

    public SettingsControllerUnitTests()
    {
        // default message sender behavior
        _messageSender.SetupGet(m => m.IsConnected).Returns(true);
        Message_sender_setup_StartAndSend();

        // IMediator setups moved to ctor per request types used by SettingsController,
        // mapping Send(...) to repository mocks.
        var saveShopSettingsCommandHandler = new SaveShopSettingsCommandHandler<IDbContextTransaction>(_shopSettingsRepository.Object, _unitOfWorkMock.Object, _publisherMock.Object);

        // SaveShopSettingsCommand -> IShopSettingsRepository.SaveShopSettings
        _mediator.Setup(m => m.Send(It.IsAny<SaveShopSettingsCommand>(), It.IsAny<CancellationToken>()))
                 .Returns((SaveShopSettingsCommand req, CancellationToken ct) =>
                     saveShopSettingsCommandHandler.Handle(req, ct));         

        var saveShopSettingsWithChildrenCommandHandler = new SaveShopSettingsWithChildrenCommandHandler<IDbContextTransaction>(_shopSettingsRepository.Object, _unitOfWorkMock.Object, _publisherMock.Object);

        // SaveShopSettingsWithChildrenCommand -> IShopSettingsRepository.SaveShopSettingsWithChildren
        _mediator.Setup(m => m.Send(It.IsAny<SaveShopSettingsWithChildrenCommand>(), It.IsAny<CancellationToken>()))
                 .Returns((SaveShopSettingsWithChildrenCommand req, CancellationToken ct) =>
                     saveShopSettingsWithChildrenCommandHandler.Handle(req, ct)!);

        // UpdateCommand<ShopSettings> -> IShopSettingsRepository.SaveShopSettings (used as update)
        _mediator.Setup(m => m.Send(It.IsAny<UpdateCommand<Alchemist.Product.Data.ShopSettings>>(), It.IsAny<CancellationToken>()))
                 .Returns((UpdateCommand<Alchemist.Product.Data.ShopSettings> req, CancellationToken ct) =>
                     _shopSettingsRepository.Object.SaveShopSettings(req.Entity, ct)!);

        // FindByNameRequest<ShopSettings> -> IRepository<ShopSettings>.FindByName
        _mediator.Setup(m => m.Send(It.IsAny<FindByNameRequest<Alchemist.Product.Data.ShopSettings>>(), It.IsAny<CancellationToken>()))
                 .Returns((FindByNameRequest<Alchemist.Product.Data.ShopSettings> req, CancellationToken ct) =>
                     _repo.Object.FindByName(req.GetName, req.Name, ct)!);

        // GetByIdRequest<Product.Data.ShopSettings> (object overload) -> IRepository.GetById<int>
        _mediator.Setup(m => m.Send(It.IsAny<GetByIdRequest<Alchemist.Product.Data.ShopSettings>>(), It.IsAny<CancellationToken>()))
                 .Returns((GetByIdRequest<Alchemist.Product.Data.ShopSettings> req, CancellationToken ct) =>
                 {
                     if (req.Id is int i)
                         return _repo.Object.GetById<int>(i, ct);
                     return Task.FromResult<Alchemist.Product.Data.ShopSettings?>(null);
                 });

        // GetChildSettingsRequest -> IShopSettingsRepository.GetChildSettings
        _mediator.Setup(m => m.Send(It.IsAny<GetChildSettingsRequest>(), It.IsAny<CancellationToken>()))
                 .Returns((GetChildSettingsRequest req, CancellationToken ct) =>
                     _shopSettingsRepository.Object.GetChildSettings(req.ParentSettingsId, ct)!);

        // GetAllParentShopSettingsRequest -> IShopSettingsRepository.GetAllParentShopSettings
        _mediator.Setup(m => m.Send(It.IsAny<GetAllParentShopSettingsRequest>(), It.IsAny<CancellationToken>()))
                 .Returns((GetAllParentShopSettingsRequest req, CancellationToken ct) =>
                     _shopSettingsRepository.Object.GetAllParentShopSettings(ct)!);

        // GetShopSettingsByShopIdRequest -> IShopSettingsRepository.GetShopSettingsByShopId
        _mediator.Setup(m => m.Send(It.IsAny<GetShopSettingsByShopIdRequest>(), It.IsAny<CancellationToken>()))
                 .Returns((GetShopSettingsByShopIdRequest req, CancellationToken ct) =>
                     _shopSettingsRepository.Object.GetShopSettingsByShopId(req.ShopId, req.SettingType, ct)!);

        _controller = new SettingsController(_logger, _mediator.Object)
        {
            Url = new Mock<IUrlHelper>().Object
        };
    }

    private void Message_sender_setup_StartAndSend()
    {
        _messageSender.Setup(m => m.Start(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _messageSender.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task GetShopSettingsByShopId_ReturnsBadRequest_WhenShopIdNegative()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopSettingsByShopId(-1, (int)ShopSettingType.Category));
        Assert.IsType<BadRequest<int>>(result.Result);
    }

    [Fact]
    public async Task GetShopSettingsByShopId_ReturnsOk_WhenRepositoryReturns()
    {
        var shopSettings = new Alchemist.Product.Data.ShopSettings { Id = 11, ShopId = 5, Name = "n" };
        _shopSettingsRepository.Setup(r => r.GetShopSettingsByShopId(5, It.IsAny<ShopSettingType>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(shopSettings);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopSettingsByShopId(5, (int)ShopSettingType.Category));
        var ok = Assert.IsType<Ok<Alchemist.Product.Data.ShopSettings>>(result.Result);
        Assert.Equal(shopSettings.Id, ok.Value.Id);
    }

    [Fact]
    public async Task GetShopSettingsById_ReturnsBadRequest_WhenIdNegative()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopSettingsById(-5));
        Assert.IsType<BadRequest<int>>(result.Result);
    }

    [Fact]
    public async Task GetShopSettingsById_ReturnsOk_WhenRepositoryReturns()
    {
        var s = new Alchemist.Product.Data.ShopSettings { Id = 7, ShopId = 2, Name = "n" };
        _repo.Setup(r => r.GetById<int>(7, It.IsAny<CancellationToken>())).ReturnsAsync(s);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopSettingsById(7));
        var ok = Assert.IsType<Ok<Alchemist.Product.Data.ShopSettings>>(result.Result);
        Assert.Equal(s.Id, ok.Value.Id);
    }

    [Fact]
    public async Task GetShopSettingsByName_ReturnsBadRequest_WhenNameEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopSettingsByName(""));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task GetShopSettingsByName_ReturnsOk_WhenRepositoryFinds()
    {
        var name = "TestName";
        var s = new Alchemist.Product.Data.ShopSettings { Id = 22, Name = name };
        _repo.Setup(r => r.FindByName(It.IsAny<Func<Alchemist.Product.Data.ShopSettings, string>>(), name, It.IsAny<CancellationToken>()))
             .ReturnsAsync(s);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopSettingsByName(name));
        var ok = Assert.IsType<Ok<Alchemist.Product.Data.ShopSettings>>(result.Result);
        Assert.Equal(s.Id, ok.Value.Id);
    }

    [Fact]
    public async Task SaveShopSettings_ReturnsBadRequest_WhenNull()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.SaveShopSettings(null));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task SaveShopSettings_ReturnsCreated_WhenNew()
    {
        var incoming = new Alchemist.Product.Data.ShopSettings { Id = 0, ShopId = 1, JsonValue = "{}", Name = "p" };
        var saved = new Alchemist.Product.Data.ShopSettings { Id = 123, ShopId = 1, JsonValue = "{}", Name = incoming.Name };

        _shopSettingsRepository.Setup(r => r.SaveShopSettings(It.IsAny<Alchemist.Product.Data.ShopSettings>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(saved);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.SaveShopSettings(incoming));
        var created = Assert.IsType<Created<Alchemist.Product.Data.ShopSettings>>(result.Result);
        Assert.Equal(saved.Id, created.Value.Id);

        _publisherMock.Verify(p => p.Publish(It.Is<CreationEvent<Alchemist.Product.Data.ShopSettings>>(c => c.Entity == created.Value), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task UpdateShopSettings_ReturnsAccepted_WhenUpdated()
    {
        var incoming = new Alchemist.Product.Data.ShopSettings { Id = 7, ShopId = 1, JsonValue = "{}", Name = "u" };
        
        _shopSettingsRepository.Setup(r => r.SaveShopSettings(It.IsAny<Alchemist.Product.Data.ShopSettings>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(incoming);

        

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.UpdateShopSettings(incoming));
        var accepted = Assert.IsType<Accepted<Alchemist.Product.Data.ShopSettings>>(result.Result);
        Assert.Equal(incoming.Id, accepted.Value.Id);

        _publisherMock.Verify(p => p.Publish(It.Is<CreationEvent<Alchemist.Product.Data.ShopSettings>>(c => c.Entity == accepted.Value), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveShopSettingsWithServices_ReturnsBadRequest_WhenInvalidPayload()
    {
        // null
        var result1 = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.SaveShopSettingsWithServices(null));
        Assert.IsType<BadRequest>(result1.Result);

        // empty list
        var result2 = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.SaveShopSettingsWithServices(new SettingsController.ShopSettingsWithServices (null, [])));
        Assert.IsType<BadRequest>(result2.Result);

        // count < 2
        var shopSettings = new SettingsController.ShopSettingsWithServices(new Alchemist.Product.Data.ShopSettings(), []);
        var result3 = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.SaveShopSettingsWithServices(shopSettings));
        var badRequest3 = Assert.IsType<BadRequest<SettingsController.ShopSettingsWithServices>>(result3.Result);
        Assert.Same(shopSettings, badRequest3.Value);
    }

    [Fact]
    public async Task SaveShopSettingsWithServices_ReturnsCreated_WhenRepositoryCreates()
    {
        // Prepare parent and two services; serialize them as strings (controller expects ToString() -> JSON)
        var parent = new Alchemist.Product.Data.ShopSettings { Id = 0, ShopId = 1, JsonValue = "{\"a\":1}", Name = "parent", Type = ShopSettingType.Category };
        var service1 = new Alchemist.Product.Data.ShopSettings { Id = 0, ShopId = 1, JsonValue = "{\"s\":1}", Name = "s1", Type = ShopSettingType.Service };
        var service2 = new Alchemist.Product.Data.ShopSettings { Id = 0, ShopId = 1, JsonValue = "{\"s\":2}", Name = "s2", Type = ShopSettingType.Service };

        var payload = new SettingsController.ShopSettingsWithServices(parent, [service1, service2]);

        // Repository will return list with parent (id assigned) and services (ids assigned)
        var savedParent = new Alchemist.Product.Data.ShopSettings { Id = 100, ShopId = parent.ShopId, JsonValue = parent.JsonValue, Name = parent.Name, Type = parent.Type };
        var savedService1 = new Alchemist.Product.Data.ShopSettings { Id = 101, ShopId = parent.ShopId, JsonValue = service1.JsonValue, Name = service1.Name, Type = service1.Type, ParentSettingsId = savedParent.Id };
        var savedService2 = new Alchemist.Product.Data.ShopSettings { Id = 102, ShopId = parent.ShopId, JsonValue = service2.JsonValue, Name = service2.Name, Type = service2.Type, ParentSettingsId = savedParent.Id };

        var saved = (savedParent, new Alchemist.Product.Data.ShopSettings[] { savedService1, savedService2 });

        _shopSettingsRepository.Setup(r => r.SaveShopSettingsWithChildren(It.IsAny<Alchemist.Product.Data.ShopSettings>(), It.IsAny<IEnumerable<Alchemist.Product.Data.ShopSettings>>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(saved);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.SaveShopSettingsWithServices(payload));
        var created = Assert.IsType<Created<ShopSettingsWithServices>>(result.Result);
        var shopSettingsWithServices = Assert.IsType<ShopSettingsWithServices>(created.Value);        
        

        var resultParent = Assert.IsType<Alchemist.Product.Data.ShopSettings>(shopSettingsWithServices.ShopSettings);
        Assert.Equal(savedParent.Id, resultParent.Id);

        // second element should be JSON array of services
        var services = Assert.IsType<Alchemist.Product.Data.ShopSettings[]>(shopSettingsWithServices.Services);
        Assert.NotNull(services);
        Assert.Equal(2, services.Length);
        Assert.All(services, s => Assert.NotEqual(0, s.Id));

        _publisherMock.Verify(p => p.Publish(It.Is<CreationEvent<Alchemist.Product.Data.ShopSettings>>(c => c.Entity == created.Value.ShopSettings), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetChildSettings_ReturnsBadRequest_WhenParentIdInvalid()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetChildSettings(0));
        Assert.IsType<BadRequest<int>>(result.Result);
    }

    [Fact]
    public async Task GetChildSettings_ReturnsOk_WhenRepositoryReturns()
    {
        var list = new List<Alchemist.Product.Data.ShopSettings> { new() { Id = 1, ParentSettingsId = 2 }, new() { Id = 2, ParentSettingsId = 2 } };
        _shopSettingsRepository.Setup(r => r.GetChildSettings(2, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetChildSettings(2));
        var ok = Assert.IsType<Ok<List<Alchemist.Product.Data.ShopSettings>>>(result.Result);
        Assert.Equal(2, ok.Value.Count);
    }

    [Fact]
    public async Task GetAllParentShopSettings_ReturnsOk()
    {
        var list = new List<Alchemist.Product.Data.ShopSettings> { new() { Id = 1 }, new() { Id = 2 } };
        _shopSettingsRepository.Setup(r => r.GetAllParentShopSettings(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetAllParentShopSettings());
        var ok = Assert.IsType<Ok<List<Alchemist.Product.Data.ShopSettings>>>(result.Result);
        Assert.Equal(2, ok.Value.Count);
    }
}