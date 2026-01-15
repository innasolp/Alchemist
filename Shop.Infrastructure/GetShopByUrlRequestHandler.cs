using MediatR;
using Shop.UnitOfWork;

namespace Shop.Infrastructure;

public class GetShopByUrlRequestHandler(IShopRepository repository) : IRequestHandler<GetShopByUrlRequest, Alchemist.Product.Data.Shop>
{
    private readonly IShopRepository _repository = repository;

    public Task<Alchemist.Product.Data.Shop> Handle(GetShopByUrlRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopByUrl(request.Url, cancellationToken)!;
    }
}