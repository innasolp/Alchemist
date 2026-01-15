using MediatR;

namespace Mediator.Infrastructure.Request;

public abstract class FindByNamePropertyRequest<T>(string name, Func<T, string[]> nameProperties) : IRequest<T>
{
    public string Name { get; } = name;

    public Func<T, string[]> NameProperties { get; } = nameProperties;
}