namespace ShopSettings.API.UnitTest;

public interface IRepository<T>
{
    Task<T> Create(T entity, CancellationToken cancellationToken = default);

    Task<T> Update(T entity, CancellationToken cancellationToken = default);

    Task<T?> GetById<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : struct;

    Task<T?> GetById(object id, CancellationToken cancellationToken = default);

    Task<T?> FindByName(Func<T, string> getName, string name, CancellationToken cancellationToken = default);

    Task<T?> FindByName(string name, Func<T, string[]> nameProperties, CancellationToken cancellationToken = default);

    Task<List<T>> GetAll(CancellationToken cancellationToken = default);
}
