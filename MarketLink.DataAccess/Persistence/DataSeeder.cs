using MarketLink.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace MarketLink.DataAccess.Persistence
{
    public class DataSeeder
    {
        private static readonly Guid CompanyRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid ShopRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Permissions — product
        private static readonly Guid ProdCreateId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        private static readonly Guid ProdReadId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        private static readonly Guid ProdUpdateId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        private static readonly Guid ProdDeleteId = Guid.Parse("10000000-0000-0000-0000-000000000004");

        // Permissions — order
        private static readonly Guid OrdCreateId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        private static readonly Guid OrdReadId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid OrdUpdateId = Guid.Parse("20000000-0000-0000-0000-000000000003");

        // Permissions — catalog
        private static readonly Guid CatReadId = Guid.Parse("30000000-0000-0000-0000-000000000001");

        // Permissions — stats
        private static readonly Guid StatsReadId = Guid.Parse("40000000-0000-0000-0000-000000000001");

        // Permissions — rating
        private static readonly Guid RatingCreateId = Guid.Parse("50000000-0000-0000-0000-000000000001");

        // Permissions — admin
        private static readonly Guid AdminApproveId = Guid.Parse("60000000-0000-0000-0000-000000000001");
        private static readonly Guid AdminBlockId = Guid.Parse("60000000-0000-0000-0000-000000000002");
        private static readonly Guid AdminAllReadId = Guid.Parse("60000000-0000-0000-0000-000000000003");

        public static async Task SeedAsync(AppDbContext ctx)
        {
            await SeedRolesAsync(ctx);
            await SeedPermissionsAsync(ctx);
            await SeedRolePermissionsAsync(ctx);
            await ctx.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(AppDbContext ctx)
        {
            var existing = (await ctx.Roles.Select(r => r.Id).ToListAsync()).ToHashSet();

            var roles = new[]
            {
                new Role { Id = CompanyRoleId,  Name = "Company", Description = "Korxona (Ishlab chiqaruvchi)" },
                new Role { Id = ShopRoleId,     Name = "Shop",    Description = "Do'kon (Xaridor)" },
                new Role { Id = AdminRoleId,    Name = "Admin",   Description = "Tizim administratori" }
            };

            foreach (var r in roles)
            {
                if (!existing.Contains(r.Id))
                    ctx.Roles.Add(r);
            }
        }
        private static async Task SeedPermissionsAsync(AppDbContext ctx)
        {
            var existing = (await ctx.Permissions.Select(p => p.Id).ToListAsync()).ToHashSet();

            var perms = new[]
            {
                // product.*
                new Permission { Id = ProdCreateId,  Name = "product.create",  Description = "Mahsulot qo'shish" },
                new Permission { Id = ProdReadId,    Name = "product.read",    Description = "Mahsulot ko'rish" },
                new Permission { Id = ProdUpdateId,  Name = "product.update",  Description = "Mahsulot tahrirlash" },
                new Permission { Id = ProdDeleteId,  Name = "product.delete",  Description = "Mahsulot o'chirish" },
                // order.*
                new Permission { Id = OrdCreateId,   Name = "order.create",    Description = "Buyurtma berish" },
                new Permission { Id = OrdReadId,     Name = "order.read",      Description = "Buyurtma ko'rish" },
                new Permission { Id = OrdUpdateId,   Name = "order.update",    Description = "Buyurtma status o'zgartirish" },
                // catalog.*
                new Permission { Id = CatReadId,     Name = "catalog.read",    Description = "Katalogni ko'rish" },
                // stats.*
                new Permission { Id = StatsReadId,   Name = "stats.read",      Description = "Statistika ko'rish" },
                // rating.*
                new Permission { Id = RatingCreateId,Name = "rating.create",   Description = "Reyting berish" },
                // admin.*
                new Permission { Id = AdminApproveId,Name = "admin.approve",   Description = "Akkaunt tasdiqlash" },
                new Permission { Id = AdminBlockId,  Name = "admin.block",     Description = "Akkaunt bloklash" },
                new Permission { Id = AdminAllReadId,Name = "admin.read_all",  Description = "Barcha ma'lumotlarni ko'rish" }
            };

            foreach (var p in perms)
            {
                if (!existing.Contains(p.Id))
                    ctx.Permissions.Add(p);
            }
        }

        private static async Task SeedRolePermissionsAsync(AppDbContext ctx)
        {
            var existing = await ctx.RolePermissions
                .Select(rp => new { rp.RoleId, rp.PermissionId })
                .ToListAsync();

            var map = new Dictionary<(Guid RoleId, Guid PermId), bool>();
            foreach (var e in existing)
                map[(e.RoleId, e.PermissionId)] = true;

            // Company: product CRUD + order.read/update + stats + rating
            var companyPerms = new[] { ProdCreateId, ProdReadId, ProdUpdateId, ProdDeleteId,
                                       OrdReadId, OrdUpdateId, StatsReadId };

            foreach (var pid in companyPerms)
                TryAdd(ctx, map, CompanyRoleId, pid);

            // Shop: catalog.read + order.create/read + product.read + rating
            var shopPerms = new[] { CatReadId, OrdCreateId, OrdReadId, ProdReadId, RatingCreateId };

            foreach (var pid in shopPerms)
                TryAdd(ctx, map, ShopRoleId, pid);

            // Admin: barcha huqlar 
            var allPermIds = new[]
            {
                ProdCreateId, ProdReadId, ProdUpdateId, ProdDeleteId,
                OrdCreateId,  OrdReadId,  OrdUpdateId,
                CatReadId, StatsReadId, RatingCreateId,
                AdminApproveId, AdminBlockId, AdminAllReadId
            };

            foreach (var pid in allPermIds)
                TryAdd(ctx, map, AdminRoleId, pid);
        }

        private static void TryAdd(
            AppDbContext ctx,
            Dictionary<(Guid, Guid), bool> existing,
            Guid roleId, Guid permId)
        {
            var key = (roleId, permId);
            if (existing.ContainsKey(key)) return;

            ctx.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });
            existing[key] = true;
        }
    }
}
