using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alchemist.Product.Data;
using Alchemist.Product.RestAPI.Controllers;
using Mediator.Infrastructure.Command;
using Mediator.Infrastructure.Request;
using MediatR;
using Message.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shop.UnitOfWork;
using Xunit;

namespace Shop.Api.UnitTest
{
    public class ShopControllerTests
    {
        private readonly ILogger<ShopController> _logger = new Logger<ShopController>(new LoggerFactory());
        private readonly Mock<IMediator> _mediator = new();
        private readonly Mock<IMessageSender> _messageSender = new();
        private readonly Mock<IShopRepository> _shopRepository = new();

        private readonly ShopController _controller;

        public ShopControllerTests()
        {
            // default message sender behavior
            _messageSender.SetupGet(m => m.IsConnected).Returns(true);
            _messageSender.Setup(m => m.Start(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _message_sender_setup_Send();

            // IMediator setups moved to ctor per request:
            // CreateCommand<Alchemist.Product.Data.Shop> -> repository.Create
            _mediator.Setup(m => m.Send(It.IsAny<CreateCommand<Alchemist.Product.Data.Shop>>(), It.IsAny<CancellationToken>()))
                     .Returns((CreateCommand<Alchemist.Product.Data.Shop> req, CancellationToken ct) => _shopRepository.Object.Create(req.Entity, ct));

            // UpdateCommand<Alchemist.Product.Data.Shop> -> repository.Update
            _mediator.Setup(m => m.Send(It.IsAny<UpdateCommand<Alchemist.Product.Data.Shop>>(), It.IsAny<CancellationToken>()))
                     .Returns((UpdateCommand<Alchemist.Product.Data.Shop> req, CancellationToken ct) => _shopRepository.Object.Update(req.Entity, ct));

            // FindByNameRequest<Alchemist.Product.Data.Shop> -> repository.FindByName
            _mediator.Setup(m => m.Send(It.IsAny<FindByNameRequest<Alchemist.Product.Data.Shop>>(), It.IsAny<CancellationToken>()))
                     .Returns((FindByNameRequest<Alchemist.Product.Data.Shop> req, CancellationToken ct) => _shopRepository.Object.FindByName(req.GetName, req.Name, ct));

            // GetAllRequest<Alchemist.Product.Data.Shop> -> repository.GetAll
            _mediator.Setup(m => m.Send(It.IsAny<GetAllRequest<Alchemist.Product.Data.Shop>>(), It.IsAny<CancellationToken>()))
                     .Returns((GetAllRequest<Alchemist.Product.Data.Shop> req, CancellationToken ct) => _shopRepository.Object.GetAll(ct));

            // GetByIdRequest<int, Alchemist.Product.Data.Shop> -> repository.GetById<int>
            _mediator.Setup(m => m.Send(It.IsAny<GetByIdRequest<int, Alchemist.Product.Data.Shop>>(), It.IsAny<CancellationToken>()))
                     .Returns((GetByIdRequest<int, Alchemist.Product.Data.Shop> req, CancellationToken ct) => _shopRepository.Object.GetById<int>(req.Id, ct));

            // Also support object-based overload GetByIdRequest<Alchemist.Product.Data.Shop>(object)
            _mediator.Setup(m => m.Send(It.IsAny<GetByIdRequest<Alchemist.Product.Data.Shop>>(), It.IsAny<CancellationToken>()))
                     .Returns((GetByIdRequest<Alchemist.Product.Data.Shop> req, CancellationToken ct) =>
                     {
                         if (req.Id is int i) return _shopRepository.Object.GetById<int>(i, ct);
                         return Task.FromResult<Alchemist.Product.Data.Shop?>(default);
                     });

            _controller = new ShopController(_logger, _mediator.Object, _messageSender.Object)
            {
                Url = new Mock<IUrlHelper>().Object
            };
        }

        private void _message_sender_setup_Send()
        {
            _messageSender.Setup(m => m.Send(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GetShopByName_ReturnsBadRequest_WhenNameEmpty()
        {
            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopByName(""));
            Assert.IsType<BadRequest>(result.Result);
        }

        [Fact]
        public async Task GetShopByName_ReturnsOk_WhenRepositoryFinds()
        {
            var name = "MyShop";
            var shop = new Alchemist.Product.Data.Shop { Id = 10, Name = name, Url = "u" };

            _shopRepository.Setup(r => r.FindByName(It.IsAny<Func<Alchemist.Product.Data.Shop, string>>(), name, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(shop);

            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShopByName(name));
            var ok = Assert.IsType<Ok<Alchemist.Product.Data.Shop>>(result.Result);
            Assert.Equal(shop.Id, ok.Value.Id);
            Assert.Equal(shop.Name, ok.Value.Name);
        }

        [Fact]
        public async Task GetShopById_ReturnsBadRequest_WhenIdInvalid()
        {
            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShop(0));
            var bad = Assert.IsType<BadRequest<int>>(result.Result);
            Assert.Equal(0, bad.Value);
        }

        [Fact]
        public async Task GetShopById_ReturnsNotFound_WhenRepositoryReturnsNull()
        {
            var id = 55;
            _shopRepository.Setup(r => r.GetById<int>(id, It.IsAny<CancellationToken>())).ReturnsAsync((Alchemist.Product.Data.Shop?)null);

            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShop(id));
            var nf = Assert.IsType<NotFound<int>>(result.Result);
            Assert.Equal(id, nf.Value);
        }

        [Fact]
        public async Task GetShops_ReturnsNotFound_WhenNoItems()
        {
            _shopRepository.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync((List<Alchemist.Product.Data.Shop>?)null);

            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShops());
            Assert.IsType<NotFound>(result.Result);
        }

        [Fact]
        public async Task GetShops_ReturnsOk_WhenItemsExist()
        {
            var items = new List<Alchemist.Product.Data.Shop> { new() { Id = 1, Name = "A" }, new() { Id = 2, Name = "B" } };
            _shopRepository.Setup(r => r.GetAll(It.IsAny<CancellationToken>())).ReturnsAsync(items);

            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.GetShops());
            var ok = Assert.IsType<Ok<List<Alchemist.Product.Data.Shop>>>(result.Result);
            Assert.Equal(2, ok.Value.Count);
        }

        [Fact]
        public async Task CreateShop_ReturnsCreated_WhenRepositoryCreates()
        {
            var incoming = new Alchemist.Product.Data.Shop { Id = 0, Name = "New", Url = "u" };
            var created = new Alchemist.Product.Data.Shop { Id = 99, Name = incoming.Name, Url = incoming.Url };

            _shopRepository.Setup(r => r.Create(It.IsAny<Alchemist.Product.Data.Shop>(), It.IsAny<CancellationToken>())).ReturnsAsync(created);

            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.CreateShop(incoming));
            var cr = Assert.IsType<Created<Alchemist.Product.Data.Shop>>(result.Result);
            Assert.Equal(created.Id, cr.Value.Id);
        }

        [Fact]
        public async Task UpdateShop_ReturnsAccepted_WhenRepositoryUpdates()
        {
            var incoming = new Alchemist.Product.Data.Shop { Id = 7, Name = "Upd", Url = "u" };
            _shopRepository.Setup(r => r.Update(It.IsAny<Alchemist.Product.Data.Shop>(), It.IsAny<CancellationToken>())).ReturnsAsync(incoming);

            var result = Assert.IsAssignableFrom<INestedHttpResult>(await _controller.UpdateShop(incoming));
            var ok = Assert.IsType<Accepted<Alchemist.Product.Data.Shop>>(result.Result);
            Assert.Equal(incoming.Id, ok.Value.Id);
        }
    }
}