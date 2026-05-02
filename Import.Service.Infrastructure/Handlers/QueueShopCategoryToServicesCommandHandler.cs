using MediatR;

namespace Import.Service.Infrastructure.Handlers;

internal sealed class QueueShopCategoryToServicesCommandHandler(IShopImportServiceManager serviceRepository) :
    IRequestHandler<QueueShopCategoryToServicesCommand>
{
    private readonly IShopImportServiceManager _serviceRepository = serviceRepository;

    public async Task Handle(QueueShopCategoryToServicesCommand request, CancellationToken cancellationToken)
    {
        await _serviceRepository.AddShopCategory(request.ShopCategory, cancellationToken);
    }
}