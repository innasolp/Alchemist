namespace Mediator.Infrastructure.Command;

public class CreateCommand<T>(T entity) : Command<T>(entity)
{
}
