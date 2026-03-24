using MarketLink.API.Attributes;
using MarketLink.API.Common;
using MarketLink.Application.Models.Role;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(
            IPermissionService permissionService,
            ILogger<RoleController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [RequirePermission("admin.read_all")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesWithPermissionsAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Rollar muvaffaqiyatli olindi",
                Data = roles
            });
        }

        [RequirePermission("admin.approve")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Ma'lumotlar noto'g'ri",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });

            var result = await _permissionService.AssignRoleAsync(req.UserId, req.RoleId);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User yoki rol topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Rol muvaffaqiyatli biriktirildi"
            });
        }

        [RequirePermission("admin.approve")]
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveRole(
            [FromQuery] Guid userId,
            [FromQuery] Guid roleId)
        {
            var result = await _permissionService.RemoveRoleAsync(userId, roleId);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Rol biriktirish topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Rol muvaffaqiyatli o‘chirildi"
            });
        }

        [RequirePermission("admin.approve")]
        [HttpPost("permissions/add")]
        public async Task<IActionResult> AddPermission([FromBody] RolePermissionRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Ma'lumotlar noto'g'ri",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });

            var result = await _permissionService.AddPermissionToRoleAsync(req.RoleId, req.PermissionId);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Rol yoki permission topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Permission muvaffaqiyatli qo‘shildi"
            });
        }

        [RequirePermission("admin.approve")]
        [HttpDelete("permissions/remove")]
        public async Task<IActionResult> RemovePermission(
            [FromQuery] Guid roleId,
            [FromQuery] Guid permissionId)
        {
            var result = await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Permission topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Permission muvaffaqiyatli o‘chirildi"
            });
        }
    }
}
