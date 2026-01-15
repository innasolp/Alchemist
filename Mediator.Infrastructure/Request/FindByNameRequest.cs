using MediatR;

namespace Mediator.Infrastructure.Request;

public class FindByNameRequest<T>(string name, Func<T, string> getName) : IRequest<T>
{
    public string Name { get; } = name;

    public Func<T, string> GetName { get; } = getName;
}
