using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF;

public static class DbContextExtensions
{
    public static async Task<T> Create<T>(this DbContext dbContext,  T entity, CancellationToken cancellationToken = default) 
        where T : class
    {
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public static async Task<TEntity?> GetById<TEntity,TId>(this DbContext dbContext, TId id, CancellationToken cancellationToken = default)
        where TEntity : class
        where TId:struct
    {
        return await dbContext.Set<TEntity>().FindAsync(id, cancellationToken);           
    }

    public static async Task<TEntity?> GetById<TEntity>(this DbContext dbContext, object id, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return await dbContext.Set<TEntity>().FindAsync(id, cancellationToken);           
    }

    public static async Task<TEntity?> FindByName<TEntity>(this DbContext dbContext, Func<TEntity, string> getName, string name, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var allEntities = await dbContext.Set<TEntity>().ToListAsync(cancellationToken);
        
        var formattedName = name.Trim().ToUpper();

        //todo stringcomparision
        var entities = allEntities.Where(e => getName(e).Trim().ToUpper() == formattedName).ToList();
        
        return entities.Count > 1
            ? throw new EntityWarningException($"multiple entities with name {name}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }

    public static async Task<TEntity?> FindByName<TEntity>(this DbContext dbContext, string name, Func<TEntity, string[]> nameProperties
        , CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var allEntities = await dbContext.Set<TEntity>().ToListAsync(cancellationToken);

        var formattedName = name.Trim().ToUpper();
        
        //todo stringcomparision
        var entities = allEntities.Where(e => nameProperties(e).Any(n => !string.IsNullOrEmpty(n) && n.Trim().ToUpper() == formattedName)).ToList();

        return entities.Count > 1
            ? throw new EntityWarningException($"multiple entities with name {name}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }
}