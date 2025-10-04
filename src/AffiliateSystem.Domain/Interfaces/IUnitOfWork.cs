namespace AffiliateSystem.Domain.Interfaces;

/// <summary>
/// Unit of Work Pattern
/// Transaction yönetimi ve repository'lerin koordinasyonu için kullanılır
/// Tüm repository'ler aynı DbContext'i paylaşır
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Değişiklikleri veritabanına kaydet
    /// </summary>
    Task<int> CompleteAsync();

    /// <summary>
    /// Transaction başlat
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Transaction'ı onayla
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Transaction'ı geri al
    /// </summary>
    Task RollbackAsync();
}