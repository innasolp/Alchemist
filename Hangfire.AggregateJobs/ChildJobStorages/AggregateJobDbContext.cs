using Microsoft.EntityFrameworkCore;

namespace Hangfire.AggregateJobs.ChildJobStorages;

public class AggregateJobDbContext : DbContext
{
    public AggregateJobDbContext()
    {
    }

    public AggregateJobDbContext(DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<JobEntry> JobEntries { get; set; }

    public virtual DbSet<ParentJobIdleSettings> ParentJobIdleSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobEntry>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("job_entry_pk");

            entity.ToTable("job_entry");

            entity.Property(e => e.JobId).HasColumnName("job_id").HasMaxLength(1024);
            entity.Property(e => e.ParentJobId).HasMaxLength(1024).HasColumnName("parent_job_id");
            entity.Property(e => e.ExecutionId).HasMaxLength(1024).HasColumnName("execution_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();


            entity.HasIndex(e => new { e.ParentJobId }).HasDatabaseName("ix_child_job_entry_parent_job_id");
        });

        modelBuilder.Entity<ParentJobIdleSettings>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("parent_job_idle_settings_pk");

            entity.ToTable("parent_job_idle_settings");

            entity.Property(e => e.JobId).HasColumnName("job_id").HasMaxLength(255).IsRequired();
            entity.Property(e => e.IdleTimeInSeconds).HasColumnName("idle_seconds").IsRequired();
        });
    }
}