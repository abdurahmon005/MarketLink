using MarketLink.Domain.Entities;

namespace MarketLink.Domain.Interfaces
{
    /// <summary>Unit of Work — barcha repositorylarni birlashtiradi va tranzaksiyani boshqaradi</summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Product> Products { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<Rating> Ratings { get; }
        IRepository<Company> Companies { get; }
        IRepository<Shop> Shops { get; }
        ICartRepository CartItems { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
