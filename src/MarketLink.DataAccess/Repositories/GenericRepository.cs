using MarketLink.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MarketLink.DataAccess.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _set;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _set     = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _set.FindAsync(new object[] { id }, ct);

        public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
            => await _set.ToListAsync(ct);

        public IQueryable<T> Query() => _set.AsQueryable();

        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _set.AddAsync(entity, ct);

        public void Update(T entity) => _set.Update(entity);

        public void Delete(T entity) => _set.Remove(entity);

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _set.AnyAsync(predicate, ct);

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _set.CountAsync(predicate, ct);
    }
}
