using Alchemist.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace Alchemist.Product.Data.Repository
{
    public static class RepositoryExtensions
    {
        public static async Task<T> Create<T>(this AlchemyContext dbContext,  T entity, CancellationToken cancellationToken = default) where T : class,new()
        {
            var added = await dbContext.Set<T>().AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return await Task.FromResult(added.Entity);
        }

        public static async Task<TEntity> GetById<TEntity,TId>(this AlchemyContext dbContext, TId id, CancellationToken cancellationToken = default)
            where TEntity : class,new()
            where TId:struct
        {
            var entity = await dbContext.Set<TEntity>().FindAsync(id, cancellationToken);
            return entity ?? throw new NotFoundException(string.Format("{0} with id={1} not found", typeof(TEntity), id),
                new Dictionary<string, object>() { { "id", id } });
        }

        public static async Task<TEntity?> FindByName<TEntity, TId>(this AlchemyContext dbContext, string name, CancellationToken cancellationToken = default)
            where TId : struct
            where TEntity : class, IEntity<TId>, new()
        {
            var allEntities = await dbContext.Set<TEntity>().ToListAsync(cancellationToken);
            var entities = allEntities.Where(e => e.Name.Trim().ToUpper() == name.Trim().ToUpper()).ToList();
            return entities.Count > 1
                ? throw new WarningException($"multiple entities with name {name}", entities.FirstOrDefault())
                : await Task.FromResult(entities.FirstOrDefault());
        }

        public static async Task<TEntity?> FindByName<TEntity, TId>(this AlchemyContext dbContext, string name, Func<TEntity, string[]> nameProperties
            , CancellationToken cancellationToken = default)
            where TId : struct
            where TEntity : class, IEntity<TId>, new()
        {
            var allEntities = await dbContext.Set<TEntity>().ToListAsync(cancellationToken);

            var compareName = name.Trim().ToUpper();
            
            //todo stringcomparision
            var entities = allEntities.Where(
                e => e.Name.Trim().ToUpper() == compareName  || nameProperties(e).Any(n => !string.IsNullOrEmpty(n) && n.Trim().ToUpper() == compareName)).ToList();

            return entities.Count > 1
                ? throw new WarningException($"multiple entities with name {name}", entities.FirstOrDefault())
                : await Task.FromResult(entities.FirstOrDefault());
        }
    }
}