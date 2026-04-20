namespace Db.Infrastructure.EF.Outbox;

internal class IdentifiedEvent<T>(string id, T entity, string eventName, DateTime creationDate) 
    : Event<T>(entity, eventName, creationDate), IIdentifiedEvent<T>
{
    public string Id =>  id;
}