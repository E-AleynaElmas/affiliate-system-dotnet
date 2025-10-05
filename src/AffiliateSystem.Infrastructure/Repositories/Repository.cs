using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using AffiliateSystem.Domain.Entities;
using AffiliateSystem.Domain.Interfaces;
using AffiliateSystem.Infrastructure.Data;

namespace AffiliateSystem.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation
/// Provides common data access methods for all entities
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Find entities matching a condition
    /// </summary>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// Get single entity matching a condition
    /// </summary>
    public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Add new entity
    /// </summary>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Add multiple entities
    /// </summary>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    /// <summary>
    /// Update entity
    /// </summary>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Remove entity (soft delete if configured)
    /// </summary>
    public virtual void Remove(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        Update(entity);
    }

    /// <summary>
    /// Remove multiple entities
    /// </summary>
    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Remove(entity);
        }
    }

    /// <summary>
    /// Check if any entity exists matching a condition
    /// </summary>
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// Count entities matching a condition
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();

        return await _dbSet.CountAsync(predicate);
    }
}