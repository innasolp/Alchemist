using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using Import.Service.Commands.Models;
using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class QueueShopCategoryToServicesCommandHandler(IServiceRepository serviceRepository) :
    IRequestHandler<QueueShopCategoryToServicesCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    public async Task Handle(QueueShopCategoryToServicesCommand request, CancellationToken cancellationToken)
    {
        var shop = _serviceRepository.ShopModels.OfType<ProductShopModel>().FirstOrDefault(s => s.Id == request.ShopCategory.ShopId);
        if (shop?.RootCategories.Any(c => c.ItemId == request.ShopCategory.ItemId) != true)
            return;

        var productShopCategory = request.ShopCategory.ToProductShopCategoryModel();
        shop?.Categories.Add(productShopCategory);

        if (_serviceRepository.Services.FirstOrDefault(s => s.Value.SourceId == shop.Id 
                    && s.Value.Service is IListener<IProductShopCategory> shopCategoryListener).Value.Service
            is IListener<IProductShopCategory> shopCategoryListener)
            await shopCategoryListener.On(productShopCategory);
    }
}