namespace AffiliateSystem.Domain.Entities;

/// <summary>
/// Base class for all entities
/// Implements DRY (Don't Repeat Yourself) principle
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary Key - Using GUID for globally unique ID
    /// Non-sequential for security, making it difficult to guess
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Record creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update date of the record
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Used for soft delete
    /// If true, record is considered deleted but not physically removed from database
    /// </summary>
    public bool IsDeleted { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}