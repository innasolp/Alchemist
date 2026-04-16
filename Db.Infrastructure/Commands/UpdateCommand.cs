namespace Db.Infrastructure.Commands;

public class UpdateCommand<T>(T entity) : Command<T>(entity)
{
}