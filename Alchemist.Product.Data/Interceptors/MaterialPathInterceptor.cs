using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Alchemist.Product.Data.Interceptors;

public class MaterialPathInterceptor : SaveChangesInterceptor
{
    private static readonly Dictionary<Type, List<int>> _changes = [];

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var materialPathEntities = context.ChangeTracker.Entries<IMaterialPathEntity>()
            .Where(e => e.State == EntityState.Unchanged)
            .GroupBy(e => e.Entity.GetType());

        foreach (var group in materialPathEntities)
        {
            if (!_changes.ContainsKey(group.Key)) _changes[group.Key] = [];
            _changes[group.Key].AddRange(group.Select(e => e.Entity.Id));
        }

        if(_changes.Count != 0)
            context.Database.SetCommandTimeout(600);

        foreach (var change in _changes)
        {
            var entityType = change.Key;
            var ids = change.Value.Distinct().Cast<object>().ToArray();
            if (ids.Length == 0) continue;

            var entityMetadata = context.Model.FindEntityType(entityType);

            if (entityMetadata == null) continue;

            var tableName = entityMetadata.GetTableName();
            var schema = entityMetadata.GetSchema() ?? "public";
            var fullTableName = $"\"{entityMetadata.GetSchema() ?? "public"}\".\"{entityMetadata.GetTableName()}\"";
            var placeholders = string.Join(",", ids.Select((_, i) => $"{{{i}}}"));

            var sql = $@"
               WITH RECURSIVE tree AS (
                    SELECT 
                        c.id, 
                        (COALESCE(p.path, '/') || c.id::text || '/')::text AS new_path,
                        ARRAY[c.id] AS path_acc -- Накопитель для защиты от циклов
                    FROM {fullTableName} c
                    LEFT JOIN {fullTableName} p ON c.parent_id = p.id
                    WHERE c.id IN ({placeholders})

                    UNION ALL

                    SELECT 
                        child.id, 
                        (t.new_path || child.id::text || '/')::text,
                        t.path_acc || child.id
                    FROM {fullTableName} child
                    INNER JOIN tree t ON child.parent_id = t.id
                    WHERE NOT child.id = ANY(t.path_acc) -- Защита от бесконечной рекурсии
                )
                UPDATE {fullTableName} AS c
                SET path = t.new_path
                FROM tree t
                WHERE c.id = t.id 
                  AND (c.path IS NULL OR c.path <> t.new_path)";

            await context.Database.ExecuteSqlRawAsync(sql, ids);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IMaterialPathEntity && (e.State == EntityState.Added || (e.State == EntityState.Modified &&
                        e.Properties.Any(p => p.Metadata.Name == nameof (IMaterialPathEntity.ParentId) && p.IsModified))))
            .ToList();

        foreach (var entry in entries)
        {
            var type = entry.Entity.GetType();
            if (!_changes.ContainsKey(type)) _changes[type] = [];

            if (entry.State == EntityState.Modified && entry.Entity is IMaterialPathEntity materialPathEntity && materialPathEntity.ParentId.HasValue)
            {
                var parentId = materialPathEntity.ParentId.Value;
                if (parentId == materialPathEntity.Id) throw new Exception("Self-reference cycle");

                var parentPath = await context.Set<IMaterialPathEntity>()
                    .Where(c => c.Id == parentId)
                    .Select(c => c.Path)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!string.IsNullOrEmpty(parentPath) && parentPath.StartsWith(materialPathEntity.Path))
                    throw new Exception("Deep cycle detected");
                
                if (entry.State == EntityState.Modified)
                _changes[type].Add(materialPathEntity.Id);
            }            
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}