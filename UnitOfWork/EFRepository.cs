using Microsoft.EntityFrameworkCore;

namespace UnitOfWork;

public class EFRepository<T, TDbContext>(TDbContext dbContext) : IRepository<T>
    where TDbContext : DbContext
    where T:class
{
    protected TDbContext Context { get; } = dbContext;

    public virtual Task<T> Create(T entity, CancellationToken cancellationToken = default)
    {
        return Context.Create(entity, cancellationToken);
    }

    public virtual Task<T?> FindByName(Func<T, string> getName, string name, CancellationToken cancellationToken = default)
    {
        return Context.FindByName(getName, name, cancellationToken);
    }

    public virtual Task<T?> FindByName(string name, Func<T, string[]> nameProperties, CancellationToken cancellationToken = default)
    {
        return Context.FindByName(name, nameProperties, cancellationToken);
    }

    public Task<List<T>> GetAll(CancellationToken cancellationToken = default)
    {
        return Context.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual Task<T?> GetById<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : struct
    {
        return Context.GetById<T, TId>(id, cancellationToken);
    }

    public virtual Task<T?> GetById(object id, CancellationToken cancellationToken = default)
    {
        return Context.GetById<T>(id, cancellationToken);
    }

    public virtual Task<T> Update(T entity, CancellationToken cancellationToken = default)
    {
        //todo
        var updated = Context.Update(entity);
        return Task.FromResult(updated.Entity);
    }
}