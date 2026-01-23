using MediatR;

namespace Import.Service.Commands.Handlers;

internal sealed class StartServiceCommandHandler(IServiceRepository serviceRepository)
    : IRequestHandler<StartServiceCommand>
{
    private readonly IServiceRepository _serviceRepository = serviceRepository;

    public async Task Handle(StartServiceCommand request, CancellationToken cancellationToken)
    {
        if (!_serviceRepository.Services.TryGetValue(request.Guid, out var serviceItem))
            throw new InvalidOperationException($"Service with id {request.Guid} not found.");

        await serviceItem.Service.Start(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, serviceItem.InnerTokenSource.Token).Token);
    }
}