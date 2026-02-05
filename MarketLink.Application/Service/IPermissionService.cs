using MarketLink.Application.Models.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);
        Task<bool> HasPermissionAsync(Guid userId, string permissionName);
        Task<bool> AssignRoleAsync(Guid userId, Guid roleId);
        Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);
        Task<bool> AddPermissionToRoleAsync(Guid roleId, Guid permissionId);
        Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
        Task<List<RoleDto>> GetAllRolesWithPermissionsAsync();
    }
}
