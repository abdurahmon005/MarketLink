using System.Linq.Expressions;

namespace MarketLink.Domain.Interfaces
{
    /// <summary>Generic repository interfeysi</summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<T>> GetAllAsync(CancellationToken ct = default);

        IQueryable<T> Query();

        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Delete(T entity);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    }
}
