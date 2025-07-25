using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.HelperServices.Permission;
using SharedLibrary.Resources;
using System.Security.Claims;


namespace SharedLibrary.Attributes
{
    public class PermissionAttribute : Attribute, IAsyncActionFilter
    {
        public string PageKey { get; }
        public string ActionKey { get; }

        public string PageName => DisplayNames.ResourceManager.GetString($"page_{PageKey}") ?? PageKey;
        public string ActionName => DisplayNames.ResourceManager.GetString($"action_{ActionKey}") ?? ActionKey;

        public PermissionAttribute(string pageKey, string actionKey)
        {
            PageKey = pageKey;
            ActionKey = actionKey;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var permissionService = context.HttpContext.RequestServices.GetRequiredService<ICheckPermissionService>();

            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.HttpContext.Response.StatusCode = 401;
                return;
            }
           
            var hasPermission = await permissionService.CheckPermission(userId, PageKey, ActionKey);

            if (!hasPermission)
            {
                context.HttpContext.Response.StatusCode = 403;
                return;
            }

            await next();
        }
    }
}
