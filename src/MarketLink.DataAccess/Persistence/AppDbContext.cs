using MarketLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.DataAccess.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserOTPs> UserOTPs { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }



        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Permission>(e =>
            {
                e.HasIndex(p => p.Name).IsUnique();
            });

            mb.Entity<Role>(e =>
            {
                e.HasIndex(r => r.Name).IsUnique();
            });

            mb.Entity<RolePermission>(e =>
            {
                e.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                e.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermisssions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            mb.Entity<UserRole>(e =>
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

                e.Property(u => u.Status)
                    .HasConversion<string>();
            });

            mb.Entity<Company>(e =>
            {
                e.HasOne(c => c.User)
                    .WithOne(u => u.Company)
                    .HasForeignKey<Company>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(c => c.FounderName)
                     .HasConversion<string>();

            });

            mb.Entity<Shop>(e =>
            {
                e.HasOne(s => s.User)
                    .WithOne(u => u.Shop)
                    .HasForeignKey<Shop>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(s => s.ShopType)
                    .HasConversion<string>();
            });
        }
    }

}

