using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using UserRoleEntity = MarketLink.Domain.Entities.UserRole;

namespace MarketLink.DataAccess.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRoleEntity> UserRoles { get; set; }
        public DbSet<UserOTPs> UserOTPs { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DeliveryTracking> DeliveryTrackings { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<ProductStockHistory> ProductStockHistories { get; set; }
        public DbSet<CompanyBranch> CompanyBranches { get; set; }
        public DbSet<CompanyDocument> CompanyDocuments { get; set; }
        public DbSet<SupplierNotification> SupplierNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Permission>(e =>
            {
                e.HasIndex(p => p.Name).IsUnique();
            });

            mb.Entity<Role>(e => e.HasIndex(r => r.Name).IsUnique());

            mb.Entity<RolePermission>(e =>
            {
                e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                e.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<UserRoleEntity>(e =>
            {
                e.HasKey(ur => new { ur.UserId, ur.RoleId });
                e.HasOne(ur => ur.User)
                     .WithMany(u => u.UserRoles)
                     .HasForeignKey(ur => ur.UserId)
                     .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<User>(e =>
            {
                e.HasIndex(u => u.PhoneNumber).IsUnique();
                e.Property(u => u.Status).HasConversion<string>();
            });

            mb.Entity<Company>(e =>
            {
                e.HasOne(c => c.User)
                    .WithOne(u => u.Company)
                    .HasForeignKey<Company>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(c => c.ProductionType).HasConversion<string>();
                // Renamed columns
                e.Property(c => c.AverageRating).HasColumnName("AverageRating");
                e.Property(c => c.CertificateUrl).HasColumnName("CertificateUrl");
            });

            mb.Entity<Shop>(e =>
            {
                e.HasOne(s => s.User)
                    .WithOne(u => u.Shop)
                    .HasForeignKey<Shop>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(s => s.ShopType).HasConversion<string>();
                e.Property(s => s.CertificateUrl).HasColumnName("CertificateUrl");
            });

            // ── Product ──
            mb.Entity<Product>(e =>
            {
                e.HasOne(p => p.Company)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(p => p.Price).HasPrecision(18, 2);
                e.Property(p => p.AverageRating).HasColumnName("AverageRating");
            });

            // ── Order ──
            mb.Entity<Order>(e =>
            {
                e.HasOne(o => o.Shop)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(o => o.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(o => o.Company)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(o => o.Status).HasConversion<string>();
                e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });

            // ── OrderItem ──
            mb.Entity<OrderItem>(e =>
            {
                e.HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
                e.Ignore(oi => oi.Subtotal);
            });

            // ── Rating ──
            mb.Entity<Rating>(e =>
            {
                e.HasOne(r => r.Product)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(r => r.Shop)
                    .WithMany(s => s.Ratings)
                    .HasForeignKey(r => r.ShopId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Order)
                    .WithMany()
                    .HasForeignKey(r => r.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(r => new { r.ShopId, r.ProductId, r.OrderId }).IsUnique();
            });

            // ── CartItem ──
            mb.Entity<CartItem>(e =>
            {
                e.HasOne(c => c.Shop)
                    .WithMany(s => s.CartItems)
                    .HasForeignKey(c => c.ShopId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.Product)
                    .WithMany()
                    .HasForeignKey(c => c.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(c => new { c.ShopId, c.ProductId }).IsUnique();
            });

            // ── DeliveryTracking ──
            mb.Entity<DeliveryTracking>(e =>
            {
                e.HasOne(d => d.Order)
                    .WithMany()
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(d => d.OrderId).IsUnique();
                e.Property(d => d.CurrentLat).HasPrecision(18, 6);
                e.Property(d => d.CurrentLng).HasPrecision(18, 6);
                e.Property(d => d.DistanceLeft).HasPrecision(18, 2);
            });

            // ── OrderStatusHistory ──
            mb.Entity<OrderStatusHistory>(e =>
            {
                e.HasOne(h => h.Order)
                    .WithMany()
                    .HasForeignKey(h => h.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(h => h.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(h => h.ChangedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(h => h.Status).HasConversion<string>();
            });

            // ── Notification ──
            mb.Entity<Notification>(e =>
            {
                e.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(n => n.Order)
                    .WithMany()
                    .HasForeignKey(n => n.OrderId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
                e.Property(n => n.Type).HasConversion<string>();
                e.HasIndex(n => new { n.UserId, n.IsRead });
            });

            // ── DeviceToken ──
            mb.Entity<DeviceToken>(e =>
            {
                e.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(d => d.Token).IsUnique();
                e.Property(d => d.Platform).HasConversion<string>();
            });

            // ── ProductStockHistory ──
            mb.Entity<ProductStockHistory>(e =>
            {
                e.HasOne(h => h.Product)
                    .WithMany()
                    .HasForeignKey(h => h.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(h => h.ChangedByUser)
                    .WithMany()
                    .HasForeignKey(h => h.ChangedBy)
                    .OnDelete(DeleteBehavior.Restrict);
                e.Property(h => h.ChangeType).HasConversion<string>();
                e.Property(h => h.Reason).HasConversion<string>();
            });

            // ── CompanyBranch ──
            mb.Entity<CompanyBranch>(e =>
            {
                e.HasOne(b => b.Company)
                    .WithMany()
                    .HasForeignKey(b => b.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CompanyDocument ──
            mb.Entity<CompanyDocument>(e =>
            {
                e.HasOne(d => d.Company)
                    .WithMany()
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.Property(d => d.Type).HasConversion<string>();
            });

            // ── SupplierNotification ──
            mb.Entity<SupplierNotification>(e =>
            {
                e.HasOne(n => n.Company)
                    .WithMany()
                    .HasForeignKey(n => n.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(n => n.RelatedOrder)
                    .WithMany()
                    .HasForeignKey(n => n.RelatedOrderId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
                e.Property(n => n.Type).HasConversion<string>();
                e.HasIndex(n => new { n.CompanyId, n.IsRead });
            });
        }
    }
}
