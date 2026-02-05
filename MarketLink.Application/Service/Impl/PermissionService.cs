using MarketLink.Application.Models.Permission;
using MarketLink.Application.Models.Role;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MarketLink.Application.Service.Impl
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        public PermissionService(AppDbContext context) => _context = context;
         
        public async Task<bool> AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
            var permExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);
            if (!roleExists || !permExists) return false;

            var already = await _context.RolePermissions
                 .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (already) return true;

            _context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId)
        {
           var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
            if (!userExists || !roleExists) return false;

            var already = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (already) return true; // nechi marta ishlasa ham ozgarmasligi uchun qoydim

            _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async  Task<List<RoleDto>> GetAllRolesWithPermissionsAsync()
        {
            var roles = await _context.Roles
                 .Include(r => r.RolePermissions)
                     .ThenInclude(rp => rp.Permission)
                 .ToListAsync();

            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description
                }).ToList()
            }).ToList();
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
        {
            var names = await _context.UserRoles
                           .Where(ur => ur.UserId == userId)
                           .SelectMany(ur => ur.Role.RolePermissions)
                           .Select(rp => rp.Permission.Name)
                           .Distinct()
                           .ToListAsync();

            return new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
        {
            return await _context.UserRoles
               .Where(ur => ur.UserId == userId)
               .AnyAsync(ur => ur.Role.RolePermissions
               .Any(rp => rp.Permission.Name == permissionName));
        }

        public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            var rp = await _context.RolePermissions
               .FirstOrDefaultAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
            if (rp == null) return false;

            _context.RolePermissions.Remove(rp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId)
        {
            var ur = await _context.UserRoles
               .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
            if (ur == null) return false;

            _context.UserRoles.Remove(ur);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
