namespace Alchemist.Product.Data.Repository;

public abstract class RepositoryBase<T, TId> : IRepository<T, TId>
     where T : class
    where TId : struct
{
    protected abstract TId GetId(T entity);
    
    protected AlchemyContext Context { get; set; }

    protected RepositoryBase(AlchemyContext context)
    {
        Context = context;
    }

    public TId Add(T entity)
    {
        var added = Context.Set<T>().Add(entity);
        Context.SaveChanges();
        return GetId(added.Entity);
    }

    public TId[] Add(T[] entities)
    {
        Context.Set<T>().AddRange(entities);
        Context.SaveChanges();
        return Context.ChangeTracker.Entries<T>().Select(t => GetId(t.Entity)).ToArray();
    }

    public bool Delete(TId entityId)
    {
        var toDelete = Context.Set<T>().FirstOrDefault(m => GetId(m) .Equals(entityId));
        if (toDelete == null) 
            return false;
        Context.Set<T>().Remove(toDelete);
        Context.SaveChanges();
        return true;
    }

    public virtual bool Delete(TId[] ids)
    {
        var toDelete = Context.Set<T>().Where(m => ids.Any(id=>id.Equals(GetId(m)))).ToList();
        if (toDelete == null)
            return false;
        Context.Set<T>().RemoveRange(toDelete);
        Context.SaveChanges();
        return true;
    }

    public virtual T Get(TId id)
    {
        return Context.Set<T>().FirstOrDefault(m => GetId(m).Equals(id));
    }

    public T[] GetAll()
    {
        return Context.Set<T>().ToArray();
    }

    public bool Update(T entity)
    {
        throw new NotImplementedException();
    }

    public bool Update(T[] entities)
    {
        throw new NotImplementedException();
    }
}
