using Alchemist.Product.Data;
using Mediator.Infrastructure.Command;
using Mediator.Infrastructure.Request;
using MediatR;
using Message.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Shop.API.Controllers;
using Shop.Infrastructure;
using Shop.UnitOfWork;
using UnitOfWork;

namespace Shop.Api.UnitTest;

public class ShopCategoryControllerTests
{
    private readonly ILogger<ShopCategoryController> _logger = new Logger<ShopCategoryController>(new LoggerFactory());
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IMessageSender> _messageSender = new();
    private readonly Mock<IShopCategoryRepository> _categoryRepository = new();

    private readonly ShopCategoryController _controller;

    public ShopCategoryControllerTests()
    {
        _messageSender.Setup(m => m.Start(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _messageSender.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Common mediator setups moved to ctor for ShopCategory:
        _mediator.Setup(m => m.Send(It.IsAny<UpdateCommand<ShopCategory>>(), It.IsAny<CancellationToken>()))
                 .Returns((UpdateCommand<ShopCategory> req, CancellationToken ct) => _categoryRepository.Object.Update(req.Entity, ct));

        _mediator.Setup(m => m.Send(It.IsAny<FindByNameRequest<ShopCategory>>(), It.IsAny<CancellationToken>()))
                 .Returns((FindByNameRequest<ShopCategory> req, CancellationToken ct) => _categoryRepository.Object.FindByName(req.GetName, req.Name, ct));

        _mediator.Setup(m => m.Send(It.IsAny<GetAllRequest<ShopCategory>>(), It.IsAny<CancellationToken>()))
                 .Returns((GetAllRequest<ShopCategory> req, CancellationToken ct) => _categoryRepository.Object.GetAll(ct));

        _mediator.Setup(m => m.Send(It.IsAny<GetByIdRequest<int, ShopCategory>>(), It.IsAny<CancellationToken>()))
                 .Returns((GetByIdRequest<int, ShopCategory> req, CancellationToken ct) => _categoryRepository.Object.GetById<int>(req.Id, ct));

        _controller = new ShopCategoryController(_logger, _mediator.Object)
        {
            Url = new Mock<IUrlHelper>().Object
        };
    }

    [Fact]
    public async Task AddShopCategory_ReturnsBadRequest_WhenNull()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.AddShopCategory(null));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task AddShopCategory_ReturnsBadRequest_WhenInvalidFields()
    {
        var invalid = new ShopCategory { ShopId = 0, ItemId = 0, Category = null! };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.AddShopCategory(invalid));
        Assert.IsType<BadRequest<ShopCategory>>(result.Result);
    }

    [Fact]
    public async Task AddShopCategory_ReturnsCreated_WhenRepositoryCreates()
    {
        var incoming = new ShopCategory { ShopId = 1, ItemId = 2, Category = "C", Id = 0 };
        var created = new ShopCategory { ShopId = incoming.ShopId, ItemId = incoming.ItemId, Category = incoming.Category, Id = 42 };

        var publisherMock = new Mock<IPublisher>();
        var unitOfWorkMock = new Mock<IUnitOfWork<IDbContextTransaction>>();

        var createShopCategoryCommandHandler = new CreateShopCategoryCommandHandler(_categoryRepository.Object, unitOfWorkMock.Object, publisherMock.Object);

        _categoryRepository.Setup(r => r.Create(It.IsAny<ShopCategory>(), It.IsAny<CancellationToken>())).ReturnsAsync(created);

        _mediator.Setup(m => m.Send(It.IsAny<CreateCommand<ShopCategory>>(), It.IsAny<CancellationToken>()))
                 .Returns((CreateCommand<ShopCategory> req, CancellationToken ct) => createShopCategoryCommandHandler.Handle(req, ct));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.AddShopCategory(incoming));
        var cr = Assert.IsType<Created<ShopCategory>>(result.Result);
        Assert.Equal(created.Id, cr.Value.Id);

        publisherMock.Verify(p => p.Publish(It.Is<CreateShopCategoryEvent>(c => c.Entity == cr.Value), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetShopCategoryByShopIdAndItemId_ReturnsNotFound_WhenRepositoryReturnsNull()
    {
        _categoryRepository.Setup(r => r.GetShopCategoryByShopIdAndItemId(1, 2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((ShopCategory?)null);

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopCategoryByShopIdAndItemId(1, 2));
        var nf = Assert.IsType<NotFound<Tuple<int, int>>>(result.Result);
        Assert.Equal(new Tuple<int, int>(1, 2), nf.Value);
    }

    [Fact]
    public async Task GetShopCategoryByShopIdAndItemId_ReturnsOk_WhenRepositoryHasItems()
    {
        var list = new List<ShopCategory> { new() { Id = 1, ShopId = 1, ItemId = 2 }, new() { Id = 2, ShopId = 1, ItemId = 3 } };

        _categoryRepository.Setup(r => r.GetShopCategoryByShopIdAndItemId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync((int shopId, int itemId, CancellationToken token) => list.FirstOrDefault(c=>c.ShopId == shopId && c.ItemId == itemId));

        _mediator.Setup(m => m.Send(It.IsAny<GetShopCategoryByShopIdAndItemIdRequest>(), It.IsAny<CancellationToken>()))
                 .Returns((GetShopCategoryByShopIdAndItemIdRequest req, CancellationToken ct) => _categoryRepository.Object.GetShopCategoryByShopIdAndItemId(req.ShopId, req.ItemId, ct));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopCategoryByShopIdAndItemId(1, 2));
        var ok = Assert.IsType<Ok<ShopCategory?>>(result.Result);

        Assert.Equal(1, ok.Value?.Id);
    }

    [Fact]
    public async Task GetShopCategories_ReturnsOk_WhenRepositoryHasItems()
    {
        var list = new List<ShopCategory> { new() { Id = 1, ShopId = 1 }, new() { Id = 2, ShopId = 1 } };

        _categoryRepository.Setup(r => r.GetShopCategories(1, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        _mediator.Setup(m => m.Send(It.IsAny<GetShopCategoriesRequest>(), It.IsAny<CancellationToken>()))
                 .Returns((GetShopCategoriesRequest req, CancellationToken ct) => _categoryRepository.Object.GetShopCategories(req.ShopId, ct));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopCategories(1));
        var ok = Assert.IsType<Ok<List<ShopCategory>>>(result.Result);
        Assert.Equal(2, ok.Value.Count);
    }

    [Fact]
    public async Task GetAllCategoryChildren_ReturnsOk_WhenRepositoryHasChildren()
    {
        var list = new List<ShopCategory> { new() { Id = 10, ParentId = 3 }, new() { Id = 11, ParentId = 3 } };

        _categoryRepository.Setup(r => r.GetAllCategoryChildren(3, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        _mediator.Setup(m => m.Send(It.IsAny<GetAllCategoryChildrenRequest>(), It.IsAny<CancellationToken>()))
                 .Returns((GetAllCategoryChildrenRequest req, CancellationToken ct) => _categoryRepository.Object.GetAllCategoryChildren(req.ParentId, ct));            

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetAllCategoryChildren(3));
        var ok = Assert.IsType<Ok<List<ShopCategory>>>(result.Result);
        Assert.Equal(2, ok.Value.Count);
    }
}