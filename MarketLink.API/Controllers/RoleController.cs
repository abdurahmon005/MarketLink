using MarketLink.API.Attributes;
using MarketLink.API.Common;
using MarketLink.Application.Models.Role;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Mvc;

namespace MarketLink.API.Controllers
{
    [Route("api/[controlller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public RoleController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [RequirePermission("admin.read_all")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _permissionService.GetAllRolesWithPermissionsAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Rollar olindi",
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
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });

            var ok = await _permissionService.AssignRoleAsync(req.UserId, req.RoleId);

            return ok ? Ok(new ApiResponse<object> { Success = true, Message = "Rol muvaffaqiyatli tasniflandi" })
                : BadRequest(new ApiResponse<object> { Success = false, Message = "User yoki rol topilmadi" });
        }

        [RequirePermission("admin.approve")]
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveRole([FromQuery] Guid userId, [FromQuery] Guid roleId)
        {
            var ok = await _permissionService.RemoveRoleAsync(userId, roleId);

            return ok ? Ok(new ApiResponse<object> { Success = true, Message = "Rol o'chirildi" })
                : NotFound(new ApiResponse<object> { Success = false, Message = "Rol tasniflash topilmadi" });
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
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });

            var ok = await _permissionService.AddPermissionToRoleAsync(req.RoleId, req.PermissionId);

            return ok
                ? Ok(new ApiResponse<object> { Success = true, Message = "Permission qo'shildi" })
                : BadRequest(new ApiResponse<object> { Success = false, Message = "Rol yoki permission topilmadi" });
        }

        [RequirePermission("admin.approve")]
        [HttpDelete("permissions/remove")]
        public async Task<IActionResult> RemovePermission([FromQuery] Guid roleId, [FromQuery] Guid permissionId)
        {
            var ok = await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);

            return ok
                ? Ok(new ApiResponse<object> { Success = true, Message = "Permission o'chirildi" })
                : NotFound(new ApiResponse<object> { Success = false, Message = "Permission topilmadi" });
        }
    }
}
