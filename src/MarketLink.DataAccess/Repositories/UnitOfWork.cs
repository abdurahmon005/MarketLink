using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Interfaces;

namespace MarketLink.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IRepository<Product> Products { get; }
        public IRepository<Order> Orders { get; }
        public IRepository<OrderItem> OrderItems { get; }
        public IRepository<Rating> Ratings { get; }
        public IRepository<Company> Companies { get; }
        public IRepository<Shop> Shops { get; }
        public ICartRepository CartItems { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context   = context;
            Products   = new GenericRepository<Product>(context);
            Orders     = new GenericRepository<Order>(context);
            OrderItems = new GenericRepository<OrderItem>(context);
            Ratings    = new GenericRepository<Rating>(context);
            Companies  = new GenericRepository<Company>(context);
            Shops      = new GenericRepository<Shop>(context);
            CartItems  = new CartRepository(context);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

        public void Dispose() => _context.Dispose();
    }
}
