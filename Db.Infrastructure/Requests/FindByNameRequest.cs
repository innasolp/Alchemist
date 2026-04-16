namespace Db.Infrastructure.Requests;

public class FindByNameRequest<T>(string name, Func<T, string> getName) : IRequest<T>
{
    public string Name { get; } = name;

    public Func<T, string> GetName { get; } = getName;
}