using Microsoft.EntityFrameworkCore;

namespace Hangfire.AggregateJobs.ChildJobStorages;

internal class ChildJobDbContext : DbContext
{
    public ChildJobDbContext()
    {
        Database.EnsureCreated();
    }

    public ChildJobDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<ChildJobEntry> ChildJobEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChildJobEntry>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("child_job_entry_pk");

            entity.ToTable("child_job_entry");

            entity.Property(e => e.JobId).HasColumnName("job_id").HasMaxLength(255);
            entity.Property(e => e.ParentJobId).HasMaxLength(1024).HasColumnName("parent_job_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.ParentCreatedAt).HasColumnName("parent_created_at").IsRequired();
        });
    }
}