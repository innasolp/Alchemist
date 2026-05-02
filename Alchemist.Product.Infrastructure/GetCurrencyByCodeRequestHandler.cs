using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class GetCurrencyByCodeRequestHandler(ICurrencyRepository repository) : IRequestHandler<GetCurrencyByCodeRequest, Currency>
{
    private readonly ICurrencyRepository _repository = repository;

    public Task<Currency> Handle(GetCurrencyByCodeRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetCurrencyByCode(request.Code, cancellationToken)!;
    }
}