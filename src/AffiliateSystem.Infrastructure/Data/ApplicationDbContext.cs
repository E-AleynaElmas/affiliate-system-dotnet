using Microsoft.EntityFrameworkCore;
using AffiliateSystem.Domain.Entities;

namespace AffiliateSystem.Infrastructure.Data;

/// <summary>
/// Main database context for the affiliate system
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }
    public DbSet<BlockedIp> BlockedIps { get; set; }
    public DbSet<ReferralLink> ReferralLinks { get; set; }

    /// <summary>
    /// Configure entity relationships and database constraints
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ReferralCode).IsUnique();

            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.ReferredBy)
                .WithMany(e => e.ReferredUsers)
                .HasForeignKey(e => e.ReferredById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(e => e.LoginAttempts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<BlockedIp>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);

            entity.HasIndex(e => e.IpAddress).IsUnique();

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ReferralLink>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);

            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName != null)
            {
                entityType.SetTableName(tableName);
            }
        }
    }

    /// <summary>
    /// Override SaveChangesAsync to handle audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}