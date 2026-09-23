
namespace Db.Infrastructure.Commands;

public abstract class Command<T>(T entity) : ICommand
{
    public T Entity { get; } = entity;
}