using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data.Repository
{
    public static class RepositoryExtensions
    {
        public static async Task<T> Create<T>(this AlchemyContext dbContext,  T entity) where T : class,new()
        {
            var added = await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return await Task.FromResult(added.Entity);
        }

        public static async Task<TEntity> GetById<TEntity,TId>(this AlchemyContext dbContext, TId id)
            where TEntity : class,new()
            where TId:struct
        {
            var entity = await dbContext.Set<TEntity>().FindAsync(id);
            return entity ?? throw new Exception(string.Format("{0} with id={1} not found", typeof(TEntity), id));
        }

        public static async Task<TEntity?> FindByName<TEntity, TId>(this AlchemyContext dbContext, string name)
            where TId : struct
            where TEntity : class, IEntity<TId>, new()
        {
            var allEntities = await dbContext.Set<TEntity>().ToListAsync();
            var entities = allEntities.Where(e => e.Name.ToLower() == name.ToLower()).ToList();
            if (entities.Count > 1)
            {
                throw new Exception($"multiple entities with name {name}");
            }
            return await Task.FromResult(entities.FirstOrDefault());
        }

        public static async Task<TEntity?> FindByName<TEntity, TId>(this AlchemyContext dbContext, string name, Func<TEntity, string[]> nameProperties )
            where TId : struct
            where TEntity : class, IEntity<TId>, new()
        {
            var allEntities = await dbContext.Set<TEntity>().ToListAsync();

            var entities = allEntities.Where(
                e => e.Name.ToLower() == name.ToLower() ||
            nameProperties(e).Any(
                n => n != null && n.ToLower() == name.ToLower())).ToList();
            if (entities.Count > 1)
            {
                throw new Exception($"multiple entities with name {name}");
            }
            return await Task.FromResult(entities.FirstOrDefault());
        }
    }
}
