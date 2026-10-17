using System.Text.Json;
using Krono.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Krono.Infrastructure.Persistence;

public class KronoDbContext : DbContext
{
    public KronoDbContext(DbContextOptions<KronoDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductionStage> ProductionStages => Set<ProductionStage>();
    public DbSet<MachineType> MachineTypes => Set<MachineType>();
    public DbSet<OperatorSkill> OperatorSkills => Set<OperatorSkill>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ScheduledStage> ScheduledStages => Set<ScheduledStage>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product & Stages
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Code).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.HasMany(p => p.Stages)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Value comparison for List<Guid>
        var guidListComparer = new ValueComparer<List<Guid>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<ProductionStage>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
            entity.Property(s => s.PredecessorStageIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions?)null) ?? new List<Guid>())
                .Metadata.SetValueComparer(guidListComparer);

            entity.HasOne(s => s.MachineType)
                .WithMany()
                .HasForeignKey(s => s.MachineTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.OperatorSkill)
                .WithMany()
                .HasForeignKey(s => s.OperatorSkillId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductionOrder & ScheduledStages
        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Code).IsRequired().HasMaxLength(50);
            entity.HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.ScheduledStages)
                .WithOne(s => s.ProductionOrder)
                .HasForeignKey(s => s.ProductionOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScheduledStage>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasOne(s => s.ProductionStage)
                .WithMany()
                .HasForeignKey(s => s.ProductionStageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MachineType
        modelBuilder.Entity<MachineType>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
        });

        // OperatorSkill
        modelBuilder.Entity<OperatorSkill>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
        });

        // WorkShift
        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
        });
    }
}
