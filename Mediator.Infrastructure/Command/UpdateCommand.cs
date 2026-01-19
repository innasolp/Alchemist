namespace Mediator.Infrastructure.Command;

public class UpdateCommand<T>(T entity) : Command<T>(entity)
{
}
