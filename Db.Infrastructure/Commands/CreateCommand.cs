namespace Db.Infrastructure.Commands;


public class CreateCommand<T>(T entity) : Command<T>(entity)
{
}