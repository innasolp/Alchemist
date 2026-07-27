using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Extensions;

public static class ContextExtensions
{
    public static void SetAddedTsColumn<T>(this EntityTypeBuilder<T> entity)
        where T : class, IAddedTsEnity
    {
        entity.Property(e => e.AddedTs).HasColumnName("added_ts");
    }

    public static void SetChangedTsColumns<T>(this EntityTypeBuilder<T> entity)
        where T : class, IAddedTsEnity, IUpdatedTsEntity
    {
        entity.Property(e => e.AddedTs).HasColumnName("added_ts");
        entity.Property(e => e.UpdatedTs).HasColumnName("update_ts");
    }
}