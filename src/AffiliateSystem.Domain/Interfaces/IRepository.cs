using System.Linq.Expressions;
using AffiliateSystem.Domain.Entities;

namespace AffiliateSystem.Domain.Interfaces;

/// <summary>
/// Generic Repository Interface
/// Repository Pattern: Veri erişim katmanını soyutlar
/// Unit of Work ile birlikte kullanılır
/// </summary>
/// <typeparam name="T">BaseEntity'den türeyen herhangi bir entity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// ID ile entity getir
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Tüm kayıtları getir
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Koşula göre kayıtları getir
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Koşula göre tek kayıt getir
    /// </summary>
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Yeni kayıt ekle
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Toplu kayıt ekle
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Kayıt güncelle
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Kayıt sil (Soft delete)
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Toplu sil
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);

    /// <summary>
    /// Koşula uyan kayıt var mı?
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Kayıt sayısını getir
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}