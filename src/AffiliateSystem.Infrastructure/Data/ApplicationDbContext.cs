using Microsoft.EntityFrameworkCore;
using AffiliateSystem.Domain.Entities;

namespace AffiliateSystem.Infrastructure.Data;

/// <summary>
/// Main database context for the affiliate system
/// Manages database connections and entity mappings
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet properties - represents tables in database
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

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Unique constraints
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ReferralCode).IsUnique();

            // Required fields
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.PasswordSalt)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            // Self-referencing relationship (User -> User)
            entity.HasOne(e => e.ReferredBy)
                .WithMany(e => e.ReferredUsers)
                .HasForeignKey(e => e.ReferredById)
                .OnDelete(DeleteBehavior.Restrict);

            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure LoginAttempt entity
        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(45); // Supports IPv6

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.FailureReason)
                .HasMaxLength(500);

            // Relationship with User
            entity.HasOne(e => e.User)
                .WithMany(e => e.LoginAttempts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for performance
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.CreatedAt);

            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure BlockedIp entity
        modelBuilder.Entity<BlockedIp>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            entity.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(500);

            // Unique constraint
            entity.HasIndex(e => e.IpAddress).IsUnique();

            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure ReferralLink entity
        modelBuilder.Entity<ReferralLink>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            // Unique constraint for referral code
            entity.HasIndex(e => e.Code).IsUnique();

            // Relationship with User
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Apply configurations for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Set table naming convention (pluralize)
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