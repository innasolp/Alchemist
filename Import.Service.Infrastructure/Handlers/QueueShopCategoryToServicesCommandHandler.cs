using Import.Service.Infrastructure;
using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class QueueShopCategoryToServicesCommandHandler(IShopImportServiceRepository serviceRepository) :
    IRequestHandler<QueueShopCategoryToServicesCommand>
{
    private readonly IShopImportServiceRepository _serviceRepository = serviceRepository;

    public async Task Handle(QueueShopCategoryToServicesCommand request, CancellationToken cancellationToken)
    {
        await _serviceRepository.AddShopCategory(request.ShopCategory);
    }
}