using MediatR;

namespace Mediator.Infrastructure.Request;

public class GetAllRequest<T> : IRequest<List<T>>
{
}