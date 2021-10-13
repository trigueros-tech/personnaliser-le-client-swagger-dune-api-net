using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Api.Infrastructure.Authorization
{
    public class CheckAuthorizationRequirementHandler : AuthorizationHandler<CheckAuthorizationRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CheckAuthorizationRequirement requirement)
        {
            // Because we applied the authorizations with the AuthorizeFilter on the controller.
            // if based on the route endpoint, it is another type to analyse
            var filterContext = context.Resource as AuthorizationFilterContext;
            var descriptor = filterContext?.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor != null)
            {
                if (!TryGetAttribute(descriptor, out RequireRightAttribute attribute))
                {
                    // If no attributes, then there are no specific rights to check.
                    context.Succeed(requirement);
                    return;
                }

                var user = context.User;
                if (await CanuserAccessAsync(user, attribute, filterContext.HttpContext.RequestAborted))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private async Task<bool> CanuserAccessAsync(ClaimsPrincipal user,
            RequireRightAttribute requireRightAttribute,
            CancellationToken cancellationToken)
        {
            var right = requireRightAttribute.Right;
            if (!string.IsNullOrWhiteSpace(right))
            {
                // Not an example: it is better to use the same logic for everyone. 
                var username = user.FindFirst("username")?.Value;
                if (username == "admin")
                {
                    return true;
                }

                // TODO : here we can get an user email or sub ID and look in database. 
                // As this class is configured as scoped in the Ioc container
                // you can inject anything you need such as DBContext, etc.
            }

            return false;
        }

        private bool TryGetAttribute(ControllerActionDescriptor descriptor,
            out RequireRightAttribute requireRightAttribute)
        {
            requireRightAttribute = descriptor.MethodInfo.GetCustomAttribute<RequireRightAttribute>()
                                    ?? descriptor.ControllerTypeInfo.GetCustomAttribute<RequireRightAttribute>();
            return requireRightAttribute != null;
        }
    }
}