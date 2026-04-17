namespace Db.Infrastructure.Requests;

public class FindByNameRequest<T>(string name) : IRequest<T>
{
    public string Name { get; } = name;
}