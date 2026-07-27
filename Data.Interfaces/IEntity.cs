namespace Data.Extensions;

public interface IEntity
{
    string Name { get; set; }
}

public interface IEntity<TId> : IEntity
    where TId : struct
{
    TId Id { get; set; }
}