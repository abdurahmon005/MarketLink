using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using UserRoleEntity = MarketLink.Domain.Entities.UserRole;

namespace MarketLink.DataAccess.Persistence
{
    public class DataSeeder
    {
        // ── Roles ──
        private static readonly Guid CompanyRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid ShopRoleId    = Guid.Parse("00000000-0000-0000-0000-000000000002");
        private static readonly Guid AdminRoleId   = Guid.Parse("00000000-0000-0000-0000-000000000003");

        // Permissions — product
        private static readonly Guid ProdCreateId   = Guid.Parse("10000000-0000-0000-0000-000000000001");
        private static readonly Guid ProdReadId     = Guid.Parse("10000000-0000-0000-0000-000000000002");
        private static readonly Guid ProdUpdateId   = Guid.Parse("10000000-0000-0000-0000-000000000003");
        private static readonly Guid ProdDeleteId   = Guid.Parse("10000000-0000-0000-0000-000000000004");
        // Permissions — order
        private static readonly Guid OrdCreateId    = Guid.Parse("20000000-0000-0000-0000-000000000001");
        private static readonly Guid OrdReadId      = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid OrdUpdateId    = Guid.Parse("20000000-0000-0000-0000-000000000003");
        // Permissions — catalog
        private static readonly Guid CatReadId      = Guid.Parse("30000000-0000-0000-0000-000000000001");
        // Permissions — stats
        private static readonly Guid StatsReadId    = Guid.Parse("40000000-0000-0000-0000-000000000001");
        // Permissions — rating
        private static readonly Guid RatingCreateId = Guid.Parse("50000000-0000-0000-0000-000000000001");
        // Permissions — admin
        private static readonly Guid AdminApproveId = Guid.Parse("60000000-0000-0000-0000-000000000001");
        private static readonly Guid AdminBlockId   = Guid.Parse("60000000-0000-0000-0000-000000000002");
        private static readonly Guid AdminAllReadId = Guid.Parse("60000000-0000-0000-0000-000000000003");

        // ── Seed User IDs ──
        private static readonly Guid AdminUserId  = Guid.Parse("AA000000-0000-0000-0000-000000000001");
        // Company users
        private static readonly Guid CompUser1Id  = Guid.Parse("BB000000-0000-0000-0000-000000000001");
        private static readonly Guid CompUser2Id  = Guid.Parse("BB000000-0000-0000-0000-000000000002");
        private static readonly Guid CompUser3Id  = Guid.Parse("BB000000-0000-0000-0000-000000000003");
        private static readonly Guid CompUser4Id  = Guid.Parse("BB000000-0000-0000-0000-000000000004");
        private static readonly Guid CompUser5Id  = Guid.Parse("BB000000-0000-0000-0000-000000000005");
        // Shop users
        private static readonly Guid ShopUser1Id  = Guid.Parse("CC000000-0000-0000-0000-000000000001");
        private static readonly Guid ShopUser2Id  = Guid.Parse("CC000000-0000-0000-0000-000000000002");
        private static readonly Guid ShopUser3Id  = Guid.Parse("CC000000-0000-0000-0000-000000000003");
        private static readonly Guid ShopUser4Id  = Guid.Parse("CC000000-0000-0000-0000-000000000004");
        private static readonly Guid ShopUser5Id  = Guid.Parse("CC000000-0000-0000-0000-000000000005");

        private static readonly Guid[] AllSeedUserIds =
        {
            AdminUserId,
            CompUser1Id, CompUser2Id, CompUser3Id, CompUser4Id, CompUser5Id,
            ShopUser1Id, ShopUser2Id, ShopUser3Id, ShopUser4Id, ShopUser5Id,
        };

        private static readonly Guid[] CompanyUserIds =
            { CompUser1Id, CompUser2Id, CompUser3Id, CompUser4Id, CompUser5Id };

        private static readonly Guid[] ShopUserIds =
            { ShopUser1Id, ShopUser2Id, ShopUser3Id, ShopUser4Id, ShopUser5Id };

        public static async Task SeedAsync(AppDbContext ctx)
        {
            await SeedRolesAsync(ctx);
            await SeedPermissionsAsync(ctx);
            await SeedRolePermissionsAsync(ctx);
            await ctx.SaveChangesAsync();

            await SeedUsersAsync(ctx);
            await SeedUserRolesAsync(ctx);
            await ctx.SaveChangesAsync();

            await SeedCompaniesAsync(ctx);
            await SeedShopsAsync(ctx);
            await ctx.SaveChangesAsync();

            await SeedProductsAsync(ctx);
            await ctx.SaveChangesAsync();

            await SeedOrdersAsync(ctx);
            await ctx.SaveChangesAsync();
        }

        // ── Roles ──────────────────────────────────────────────────────────────
        private static async Task SeedRolesAsync(AppDbContext ctx)
        {
            var existing = (await ctx.Roles.Select(r => r.Id).ToListAsync()).ToHashSet();

            var roles = new[]
            {
                new Role { Id = CompanyRoleId, Name = "Company", Description = "Korxona (Ishlab chiqaruvchi)" },
                new Role { Id = ShopRoleId,    Name = "Shop",    Description = "Do'kon (Xaridor)" },
                new Role { Id = AdminRoleId,   Name = "Admin",   Description = "Tizim administratori" },
            };

            foreach (var r in roles)
                if (!existing.Contains(r.Id))
                    ctx.Roles.Add(r);
        }

        // ── Permissions ────────────────────────────────────────────────────────
        private static async Task SeedPermissionsAsync(AppDbContext ctx)
        {
            var existing = (await ctx.Permissions.Select(p => p.Id).ToListAsync()).ToHashSet();

            var perms = new[]
            {
                new Permission { Id = ProdCreateId,   Name = "product.create",  Description = "Mahsulot qo'shish" },
                new Permission { Id = ProdReadId,     Name = "product.read",    Description = "Mahsulot ko'rish" },
                new Permission { Id = ProdUpdateId,   Name = "product.update",  Description = "Mahsulot tahrirlash" },
                new Permission { Id = ProdDeleteId,   Name = "product.delete",  Description = "Mahsulot o'chirish" },
                new Permission { Id = OrdCreateId,    Name = "order.create",    Description = "Buyurtma berish" },
                new Permission { Id = OrdReadId,      Name = "order.read",      Description = "Buyurtma ko'rish" },
                new Permission { Id = OrdUpdateId,    Name = "order.update",    Description = "Buyurtma status o'zgartirish" },
                new Permission { Id = CatReadId,      Name = "catalog.read",    Description = "Katalogni ko'rish" },
                new Permission { Id = StatsReadId,    Name = "stats.read",      Description = "Statistika ko'rish" },
                new Permission { Id = RatingCreateId, Name = "rating.create",   Description = "Reyting berish" },
                new Permission { Id = AdminApproveId, Name = "admin.approve",   Description = "Akkaunt tasdiqlash" },
                new Permission { Id = AdminBlockId,   Name = "admin.block",     Description = "Akkaunt bloklash" },
                new Permission { Id = AdminAllReadId, Name = "admin.read_all",  Description = "Barcha ma'lumotlarni ko'rish" },
            };

            foreach (var p in perms)
                if (!existing.Contains(p.Id))
                    ctx.Permissions.Add(p);
        }

        // ── RolePermissions ────────────────────────────────────────────────────
        private static async Task SeedRolePermissionsAsync(AppDbContext ctx)
        {
            var existing = await ctx.RolePermissions
                .Select(rp => new { rp.RoleId, rp.PermissionId })
                .ToListAsync();

            var map = new Dictionary<(Guid, Guid), bool>();
            foreach (var e in existing)
                map[(e.RoleId, e.PermissionId)] = true;

            var companyPerms = new[] { ProdCreateId, ProdReadId, ProdUpdateId, ProdDeleteId,
                                       OrdReadId, OrdUpdateId, StatsReadId };
            foreach (var pid in companyPerms)
                TryAddRolePerm(ctx, map, CompanyRoleId, pid);

            var shopPerms = new[] { CatReadId, OrdCreateId, OrdReadId, ProdReadId, RatingCreateId };
            foreach (var pid in shopPerms)
                TryAddRolePerm(ctx, map, ShopRoleId, pid);

            var allPermIds = new[]
            {
                ProdCreateId, ProdReadId, ProdUpdateId, ProdDeleteId,
                OrdCreateId, OrdReadId, OrdUpdateId,
                CatReadId, StatsReadId, RatingCreateId,
                AdminApproveId, AdminBlockId, AdminAllReadId,
            };
            foreach (var pid in allPermIds)
                TryAddRolePerm(ctx, map, AdminRoleId, pid);
        }

        private static void TryAddRolePerm(
            AppDbContext ctx,
            Dictionary<(Guid, Guid), bool> existing,
            Guid roleId, Guid permId)
        {
            var key = (roleId, permId);
            if (existing.ContainsKey(key)) return;
            ctx.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });
            existing[key] = true;
        }

        // ── Users ──────────────────────────────────────────────────────────────
        private static async Task SeedUsersAsync(AppDbContext ctx)
        {
            var now = DateTime.UtcNow;

            string Hash(string pw) => BCrypt.Net.BCrypt.HashPassword(pw);

            // (id, phone, passwordHash)
            var seedUsers = new[]
            {
                (Id: AdminUserId,  Phone: "998901000001", Hash: Hash("Admin@123")),
                (Id: CompUser1Id,  Phone: "998901000011", Hash: Hash("Company@123")),
                (Id: CompUser2Id,  Phone: "998901000012", Hash: Hash("Company@123")),
                (Id: CompUser3Id,  Phone: "998901000013", Hash: Hash("Company@123")),
                (Id: CompUser4Id,  Phone: "998901000014", Hash: Hash("Company@123")),
                (Id: CompUser5Id,  Phone: "998901000015", Hash: Hash("Company@123")),
                (Id: ShopUser1Id,  Phone: "998901000021", Hash: Hash("Shop@123")),
                (Id: ShopUser2Id,  Phone: "998901000022", Hash: Hash("Shop@123")),
                (Id: ShopUser3Id,  Phone: "998901000023", Hash: Hash("Shop@123")),
                (Id: ShopUser4Id,  Phone: "998901000024", Hash: Hash("Shop@123")),
                (Id: ShopUser5Id,  Phone: "998901000025", Hash: Hash("Shop@123")),
            };

            var allIds = seedUsers.Select(s => s.Id).ToList();
            var existing = await ctx.Users
                .Where(u => allIds.Contains(u.Id))
                .ToListAsync();
            var existingMap = existing.ToDictionary(u => u.Id);

            foreach (var (id, phone, hash) in seedUsers)
            {
                if (existingMap.TryGetValue(id, out var dbUser))
                {
                    // Fix phone if stored with '+' prefix or wrong format
                    if (dbUser.PhoneNumber != phone)
                    {
                        dbUser.PhoneNumber = phone;
                        dbUser.UpdatedAt   = now;
                    }
                }
                else
                {
                    ctx.Users.Add(new User
                    {
                        Id = id, PhoneNumber = phone,
                        IsPhoneVerified = true, PasswordHash = hash,
                        Status = UserStatus.Approved, CreatedAt = now, UpdatedAt = now,
                    });
                }
            }
        }

        // ── UserRoles ──────────────────────────────────────────────────────────
        private static async Task SeedUserRolesAsync(AppDbContext ctx)
        {
            var existing = await ctx.UserRoles
                .Select(ur => new { ur.UserId, ur.RoleId })
                .ToListAsync();
            var map = existing.ToDictionary(x => (x.UserId, x.RoleId), _ => true);

            void TryAdd(Guid userId, Guid roleId)
            {
                if (map.ContainsKey((userId, roleId))) return;
                ctx.UserRoles.Add(new UserRoleEntity { UserId = userId, RoleId = roleId });
                map[(userId, roleId)] = true;
            }

            TryAdd(AdminUserId, AdminRoleId);
            foreach (var id in CompanyUserIds) TryAdd(id, CompanyRoleId);
            foreach (var id in ShopUserIds)    TryAdd(id, ShopRoleId);
        }

        // ── Companies (Ta'minotchilar) ─────────────────────────────────────────
        private static async Task SeedCompaniesAsync(AppDbContext ctx)
        {
            var existingUserIds = (await ctx.Companies.Select(c => c.UserId).ToListAsync()).ToHashSet();
            var now = DateTime.UtcNow;

            var companies = new[]
            {
                new Company
                {
                    UserId = CompUser1Id,
                    FounderName    = "Abdullayev Jamshid Karimovich",
                    CompanyName    = "UzDairy LLC",
                    Address        = "Toshkent sh., Yunusobod tumani, Amir Temur ko'chasi 45-uy",
                    ProductionType = CompanyDirection.Dairy,
                    Description    = "O'zbekistonning yetakchi sut mahsulotlari ishlab chiqaruvchisi. Sut, qatiq, kefir, sariyog', qaymoq va tvorog kabi mahsulotlar ishlab chiqariladi.",
                    AverageRating  = 4.7,
                    CreatedAt = now, UpdatedAt = now,
                },
                new Company
                {
                    UserId = CompUser2Id,
                    FounderName    = "Rahimov Sardor Toshmatovich",
                    CompanyName    = "Baraka Non Kombinati",
                    Address        = "Samarqand sh., Registon ko'chasi 12-uy",
                    ProductionType = CompanyDirection.Bakery,
                    Description    = "Samarqandning an'anaviy non mahsulotlari ishlab chiqaruvchisi. Tandirda pishirilgan non, lepyoshka, kulcha va lavash.",
                    AverageRating  = 4.5,
                    CreatedAt = now, UpdatedAt = now,
                },
                new Company
                {
                    UserId = CompUser3Id,
                    FounderName    = "Yusupov Alisher Normatovich",
                    CompanyName    = "SharbatCo Ichimliklar",
                    Address        = "Toshkent sh., Sergeli tumani, Yangi hayot ko'chasi 7-uy",
                    ProductionType = CompanyDirection.Beverages,
                    Description    = "Tabiiy meva sharbatlari, mineral suv va limonadlar ishlab chiqaruvchisi. 100% tabiiy ingredientlar, konservantlarsiz.",
                    AverageRating  = 4.3,
                    CreatedAt = now, UpdatedAt = now,
                },
                new Company
                {
                    UserId = CompUser4Id,
                    FounderName    = "Karimova Nilufar Bekmurodovna",
                    CompanyName    = "Halva Plus Qandolat",
                    Address        = "Buxoro sh., Mustaqillik ko'chasi 23-uy",
                    ProductionType = CompanyDirection.Confectionery,
                    Description    = "An'anaviy o'zbek qandolat mahsulotlari: halva, parvarda, navat, qiyom va shakarqand. Buxoro an'analari asosida ishlab chiqariladi.",
                    AverageRating  = 4.8,
                    CreatedAt = now, UpdatedAt = now,
                },
                new Company
                {
                    UserId = CompUser5Id,
                    FounderName    = "Mirzayev Bobur Hamidovich",
                    CompanyName    = "GallaZot Yarimtayyor",
                    Address        = "Namangan sh., Uychi ko'chasi 34-uy",
                    ProductionType = CompanyDirection.SemiFinished,
                    Description    = "Muzlatilgan yarim tayyor milliy taomlar: chuchvara, manti, somsa, mastava to'plami va plov uchun tayyor to'plam.",
                    AverageRating  = 4.2,
                    CreatedAt = now, UpdatedAt = now,
                },
            };

            foreach (var c in companies)
                if (!existingUserIds.Contains(c.UserId))
                    ctx.Companies.Add(c);
        }

        // ── Shops (Sotuvchilar) ────────────────────────────────────────────────
        private static async Task SeedShopsAsync(AppDbContext ctx)
        {
            var existingUserIds = (await ctx.Shops.Select(s => s.UserId).ToListAsync()).ToHashSet();
            var now = DateTime.UtcNow;

            var shops = new[]
            {
                new Shop
                {
                    UserId = ShopUser1Id,
                    FounderName = "Toshmatov Ulug'bek Sobirovich",
                    ShopName    = "Navro'z Supermarket",
                    Address     = "Toshkent sh., Chilonzor tumani, 9-kvartal, 15-uy",
                    ShopType    = ShopType.Supermarket,
                    Description = "Toshkentning yirik supermarketlaridan biri. Oziq-ovqat, sut mahsulotlari va maishiy tovarlarning keng assortimenti.",
                    CreatedAt = now, UpdatedAt = now,
                },
                new Shop
                {
                    UserId = ShopUser2Id,
                    FounderName = "Xoliqov Eldor Mansurovich",
                    ShopName    = "Baraka Do'koni",
                    Address     = "Samarqand sh., Gumbaz ko'chasi 5-uy",
                    ShopType    = ShopType.Grocery,
                    Description = "Mahalliy aholiga xizmat qiluvchi oziq-ovqat do'koni. Har kuni yangi non va sut mahsulotlari.",
                    CreatedAt = now, UpdatedAt = now,
                },
                new Shop
                {
                    UserId = ShopUser3Id,
                    FounderName = "Nazarova Maftuna Ilhomovna",
                    ShopName    = "Ezgulik Oziq-Ovqat",
                    Address     = "Namangan sh., Bobur ko'chasi 67-uy",
                    ShopType    = ShopType.Grocery,
                    Description = "Namangandagi ishonchli oziq-ovqat do'koni. Sifatli mahsulotlar, hamyonbop narxlar.",
                    CreatedAt = now, UpdatedAt = now,
                },
                new Shop
                {
                    UserId = ShopUser4Id,
                    FounderName = "Qodirov Sherzod Baxtiyorovich",
                    ShopName    = "Al-Amin Market",
                    Address     = "Buxoro sh., Ark ko'chasi 3-uy",
                    ShopType    = ShopType.Supermarket,
                    Description = "Buxorodagi zamonaviy supermarket. Keng assortiment va qulay xizmat ko'rsatish.",
                    CreatedAt = now, UpdatedAt = now,
                },
                new Shop
                {
                    UserId = ShopUser5Id,
                    FounderName = "Ergashev Firdavs Obidovich",
                    ShopName    = "Farovon Supermarket",
                    Address     = "Farg'ona sh., Mustaqillik xiyoboni 18-uy",
                    ShopType    = ShopType.Supermarket,
                    Description = "Farg'onadagi eng yirik oziq-ovqat supermarketi. 24/7 xizmat, bepul yetkazib berish.",
                    CreatedAt = now, UpdatedAt = now,
                },
            };

            foreach (var s in shops)
                if (!existingUserIds.Contains(s.UserId))
                    ctx.Shops.Add(s);
        }

        // ── Products (Mahsulotlar) ─────────────────────────────────────────────
        private static async Task SeedProductsAsync(AppDbContext ctx)
        {
            if (await ctx.Products.AnyAsync()) return;

            var compMap = await ctx.Companies
                .Where(c => CompanyUserIds.Contains(c.UserId))
                .ToDictionaryAsync(c => c.UserId, c => c.Id);

            if (compMap.Count < 5) return; // companies not seeded yet

            int dairy    = compMap[CompUser1Id]; // UzDairy
            int bakery   = compMap[CompUser2Id]; // Baraka Non
            int beverage = compMap[CompUser3Id]; // SharbatCo
            int confect  = compMap[CompUser4Id]; // Halva Plus
            int semi     = compMap[CompUser5Id]; // GallaZot

            var now = DateTime.UtcNow;

            var products = new[]
            {
                // ── UzDairy (Sut mahsulotlari) ──────────────────────────────
                new Product { CompanyId = dairy, Name = "Sut 3.2%",        Description = "Toza sigir suti, yog'lilik 3.2%, 1 litrlik paket",               Price = 8_500,  PackageSize = 12, StockQuantity = 200, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = dairy, Name = "Qatiq klassik",   Description = "Tabiiy qatiq, yog'lilik 3.5%, 500g idish",                        Price = 7_200,  PackageSize = 12, StockQuantity = 150, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = dairy, Name = "Kefir 1%",        Description = "Fermentlangan sut ichimlik, yog'lilik 1%, 1 litr",                 Price = 9_000,  PackageSize = 8,  StockQuantity = 120, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = dairy, Name = "Sariyog' 72.5%",  Description = "Tabiiy sariyog', yog'lilik 72.5%, 200g briketi",                   Price = 22_000, PackageSize = 20, StockQuantity = 80,  IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = dairy, Name = "Qaymoq 20%",      Description = "Tabiiy qaymoq, yog'lilik 20%, 250g idish",                         Price = 14_500, PackageSize = 10, StockQuantity = 60,  IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = dairy, Name = "Tvorog 9%",       Description = "Toza tvorog, yog'lilik 9%, 400g qadoq",                            Price = 18_000, PackageSize = 6,  StockQuantity = 90,  IsActive = true, CreatedAt = now, UpdatedAt = now },

                // ── Baraka Non (Non mahsulotlari) ───────────────────────────
                new Product { CompanyId = bakery, Name = "Samarqand noni", Description = "An'anaviy tandirda pishirilgan Samarqand noni, 700g",              Price = 6_000,  PackageSize = 10, StockQuantity = 300, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = bakery, Name = "Lepyoshka",      Description = "Bug'doy unidan pishirilgan lepyoshka, 500g",                        Price = 4_500,  PackageSize = 10, StockQuantity = 250, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = bakery, Name = "Gala non",       Description = "Sog'lom gala-donlardan pishirilgan non, 400g",                      Price = 8_500,  PackageSize = 8,  StockQuantity = 120, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = bakery, Name = "Kulcha",         Description = "Tandirda pishirilgan an'anaviy kulcha, 350g",                       Price = 5_000,  PackageSize = 12, StockQuantity = 200, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = bakery, Name = "Non lavash",     Description = "Yupqa lavash non, 250g",                                           Price = 3_500,  PackageSize = 20, StockQuantity = 350, IsActive = true, CreatedAt = now, UpdatedAt = now },

                // ── SharbatCo (Ichimliklar) ──────────────────────────────────
                new Product { CompanyId = beverage, Name = "Shaftoli sharbati", Description = "100% tabiiy shaftoli sharbati, konservant yo'q, 1 litr",      Price = 15_000, PackageSize = 12, StockQuantity = 180, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = beverage, Name = "O'rik sharbati",    Description = "Tabiiy o'rik sharbati, 1 litr tetra-pak",                      Price = 14_000, PackageSize = 12, StockQuantity = 200, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = beverage, Name = "Qovun sharbati",    Description = "Sezonal qovun sharbati, 0.75 litr",                            Price = 12_000, PackageSize = 12, StockQuantity = 100, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = beverage, Name = "Mineral suv",       Description = "Tabiiy mineral suv, 1.5 litr shisha",                          Price = 5_000,  PackageSize = 6,  StockQuantity = 500, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = beverage, Name = "Limonad \"Toshkent\"", Description = "O'zbek limonadi, assortiment ta'mlar, 1 litr",              Price = 8_000,  PackageSize = 12, StockQuantity = 300, IsActive = true, CreatedAt = now, UpdatedAt = now },

                // ── Halva Plus (Qandolat) ────────────────────────────────────
                new Product { CompanyId = confect, Name = "Buxoro halvasi",  Description = "An'anaviy Buxoro halvasi, susam asosida, 500g qadoq",             Price = 45_000, PackageSize = 6,  StockQuantity = 80,  IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = confect, Name = "Parvarda",        Description = "Tabiiy parvarda, asal qo'shilgan, 300g",                          Price = 28_000, PackageSize = 10, StockQuantity = 120, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = confect, Name = "Navat",           Description = "Toza shakar navat, 250g torbada",                                  Price = 32_000, PackageSize = 8,  StockQuantity = 90,  IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = confect, Name = "Qiyom (anjir)",   Description = "An'anaviy anjir qiyomi, 350g shisha idish",                        Price = 38_000, PackageSize = 6,  StockQuantity = 60,  IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = confect, Name = "Shakarqand",      Description = "Tabiiy shakarqand konfeti, asal va yong'oq bilan, 500g",           Price = 52_000, PackageSize = 4,  StockQuantity = 50,  IsActive = true, CreatedAt = now, UpdatedAt = now },

                // ── GallaZot (Yarim tayyor mahsulotlar) ─────────────────────
                new Product { CompanyId = semi, Name = "Chuchvara (muzlatilgan)", Description = "Qo'l bilan tayyorlangan chuchvara, mol go'shtli, 1 kg",       Price = 35_000, PackageSize = 6, StockQuantity = 150, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = semi, Name = "Manti (muzlatilgan)",     Description = "An'anaviy o'zbek manti, qo'y-mol go'shtli, 12 dona/paket",   Price = 42_000, PackageSize = 4, StockQuantity = 100, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = semi, Name = "Somsa (muzlatilgan)",     Description = "Tandir somsa, go'shtli, 10 dona/paket",                        Price = 48_000, PackageSize = 4, StockQuantity = 120, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = semi, Name = "Mastava to'plami",        Description = "Yarim tayyor mastava sho'rva to'plami, sabzavot+go'sht, 500g", Price = 22_000, PackageSize = 10, StockQuantity = 80, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new Product { CompanyId = semi, Name = "Plov to'plami",           Description = "Plov uchun tayyor to'plam: guruch, piyoz, sabzi, ziravorlar, 2 kg", Price = 65_000, PackageSize = 4, StockQuantity = 70, IsActive = true, CreatedAt = now, UpdatedAt = now },
            };

            ctx.Products.AddRange(products);
        }

        // ── Orders (Buyurtmalar) ───────────────────────────────────────────────
        private static async Task SeedOrdersAsync(AppDbContext ctx)
        {
            if (await ctx.Orders.AnyAsync()) return;

            var compMap = await ctx.Companies
                .Where(c => CompanyUserIds.Contains(c.UserId))
                .ToDictionaryAsync(c => c.UserId, c => c.Id);

            var shopMap = await ctx.Shops
                .Where(s => ShopUserIds.Contains(s.UserId))
                .ToDictionaryAsync(s => s.UserId, s => s.Id);

            if (compMap.Count < 5 || shopMap.Count < 5) return;

            // Group products by companyId for quick lookup
            var companyIds = compMap.Values.ToList();
            var allProducts = await ctx.Products
                .Where(p => companyIds.Contains(p.CompanyId))
                .Select(p => new { p.Id, p.CompanyId, p.Price })
                .ToListAsync();

            var prodsByComp = allProducts
                .GroupBy(p => p.CompanyId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var now = DateTime.UtcNow;

            // Helper: build order items and compute TotalAmount
            List<OrderItem> MakeItems(int companyId, params (int idx, int qty)[] picks)
            {
                var items = new List<OrderItem>();
                if (!prodsByComp.TryGetValue(companyId, out var prods)) return items;
                foreach (var (idx, qty) in picks)
                {
                    if (idx >= prods.Count) continue;
                    items.Add(new OrderItem
                    {
                        ProductId = prods[idx].Id,
                        Quantity  = qty,
                        UnitPrice = prods[idx].Price,
                    });
                }
                return items;
            }

            decimal Total(List<OrderItem> items) => items.Sum(i => i.Quantity * i.UnitPrice);

            int dairy    = compMap[CompUser1Id];
            int bakery   = compMap[CompUser2Id];
            int beverage = compMap[CompUser3Id];
            int confect  = compMap[CompUser4Id];
            int semi     = compMap[CompUser5Id];

            int shop1 = shopMap[ShopUser1Id];
            int shop2 = shopMap[ShopUser2Id];
            int shop3 = shopMap[ShopUser3Id];
            int shop4 = shopMap[ShopUser4Id];
            int shop5 = shopMap[ShopUser5Id];

            // Order 1 — Navro'z Supermarket → UzDairy (yetkazilgan)
            var items1 = MakeItems(dairy, (0, 50), (1, 40), (3, 20)); // Sut, Qatiq, Sariyog'
            var order1 = new Order
            {
                ShopId = shop1, CompanyId = dairy,
                Status = OrderStatus.Delivered,
                DeliveryDate    = now.AddDays(-5),
                DeliveryAddress = "Toshkent sh., Chilonzor tumani, 9-kvartal, 15-uy",
                Note            = "Ertangi ertalab yetkazib berish",
                TotalAmount     = Total(items1),
                Items           = items1,
                CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-5),
            };

            // Order 2 — Baraka Do'koni → Baraka Non (qabul qilingan)
            var items2 = MakeItems(bakery, (0, 100), (1, 80), (4, 60)); // Samarqand noni, Lepyoshka, Lavash
            var order2 = new Order
            {
                ShopId = shop2, CompanyId = bakery,
                Status = OrderStatus.Accepted,
                DeliveryDate    = now.AddDays(2),
                DeliveryAddress = "Samarqand sh., Gumbaz ko'chasi 5-uy",
                Note            = "Ertalab soat 8 gacha yetkazish",
                TotalAmount     = Total(items2),
                Items           = items2,
                CreatedAt = now.AddDays(-1), UpdatedAt = now,
            };

            // Order 3 — Ezgulik Oziq-Ovqat → SharbatCo (tayyorlanmoqda)
            var items3 = MakeItems(beverage, (0, 60), (3, 100), (4, 50)); // Shaftoli, Mineral suv, Limonad
            var order3 = new Order
            {
                ShopId = shop3, CompanyId = beverage,
                Status = OrderStatus.Preparing,
                DeliveryDate    = now.AddDays(3),
                DeliveryAddress = "Namangan sh., Bobur ko'chasi 67-uy",
                TotalAmount     = Total(items3),
                Items           = items3,
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-1),
            };

            // Order 4 — Al-Amin Market → Halva Plus (kutilmoqda)
            var items4 = MakeItems(confect, (0, 20), (1, 30), (2, 15)); // Halva, Parvarda, Navat
            var order4 = new Order
            {
                ShopId = shop4, CompanyId = confect,
                Status = OrderStatus.Pending,
                DeliveryDate    = now.AddDays(7),
                DeliveryAddress = "Buxoro sh., Ark ko'chasi 3-uy",
                Note            = "Bayram uchun katta miqdorda buyurtma",
                TotalAmount     = Total(items4),
                Items           = items4,
                CreatedAt = now, UpdatedAt = now,
            };

            // Order 5 — Farovon Supermarket → GallaZot (yetkazilgan)
            var items5 = MakeItems(semi, (0, 80), (1, 50), (2, 40)); // Chuchvara, Manti, Somsa
            var order5 = new Order
            {
                ShopId = shop5, CompanyId = semi,
                Status = OrderStatus.Delivered,
                DeliveryDate    = now.AddDays(-3),
                DeliveryAddress = "Farg'ona sh., Mustaqillik xiyoboni 18-uy",
                TotalAmount     = Total(items5),
                Items           = items5,
                CreatedAt = now.AddDays(-8), UpdatedAt = now.AddDays(-3),
            };

            // Order 6 — Navro'z Supermarket → GallaZot (ikkinchi buyurtma, qabul qilingan)
            var items6 = MakeItems(semi, (3, 60), (4, 30)); // Mastava to'plami, Plov to'plami
            var order6 = new Order
            {
                ShopId = shop1, CompanyId = semi,
                Status = OrderStatus.Accepted,
                DeliveryDate    = now.AddDays(4),
                DeliveryAddress = "Toshkent sh., Chilonzor tumani, 9-kvartal, 15-uy",
                Note            = "Imkon qadar tez yetkazish so'raladi",
                TotalAmount     = Total(items6),
                Items           = items6,
                CreatedAt = now.AddDays(-1), UpdatedAt = now,
            };

            // Order 7 — Baraka Do'koni → UzDairy (rad etilgan)
            var items7 = MakeItems(dairy, (0, 30), (4, 20)); // Sut, Qaymoq
            var order7 = new Order
            {
                ShopId = shop2, CompanyId = dairy,
                Status = OrderStatus.Rejected,
                DeliveryDate    = now.AddDays(-15),
                DeliveryAddress = "Samarqand sh., Gumbaz ko'chasi 5-uy",
                Note            = "Noto'g'ri manzil ko'rsatilgan",
                TotalAmount     = Total(items7),
                Items           = items7,
                CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-15),
            };

            ctx.Orders.AddRange(order1, order2, order3, order4, order5, order6, order7);
        }
    }
}
