using MarketLink.Application.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MarketLink.API.Attributes
{
    public class PermissionActionFilter
    {
        private readonly IPermissionService _permService;

        public PermissionActionFilter(IPermissionService permService)
        {
            _permService = permService;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var descriptors = context.ActionDescriptor.EndpointMetadata
                 .OfType<RequirePermissionAttribute>()
                 .ToList();


            if (descriptors.Count == 0)
            {
                if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad)
                {
                    descriptors = cad.MethodInfo
                        .GetCustomAttributes(typeof(RequirePermissionAttribute), false)
                        .Cast<RequirePermissionAttribute>()
                        .ToList();
                }
            }

            if (descriptors.Count == 0)
            {
                await next();
                return;
            }

            // 2) UserId claimdan ochirish
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    Success = false,
                    Message = "Autentifikatsiya kerak"
                });
                return;
            }

            // 3) Userni xamma permissioni
            var userPerms = await _permService.GetUserPermissionsAsync(userId);

            // 4) xamma soralgan permissionlar bormi yoqmi tekshirish uchun
            var required = descriptors.SelectMany(d => d.Permissions).Distinct();

            foreach (var perm in required)
            {
                if (!userPerms.Contains(perm))
                {
                    context.Result = new ForbiddenObjectResult(new
                    {
                        Success = false,
                        Message = "Bu harakatra hukm yo'q",
                        Required = perm
                    });
                    return;
                }
            }

            await next();
        }
    }
}
