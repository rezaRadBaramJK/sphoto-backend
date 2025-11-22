using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Filters
{
    public sealed class PermissionAttribute : TypeFilterAttribute
    {
        public PermissionAttribute(params string[] permissions) :
            base(typeof(AuthorizePermissionFilter))
        {
            Arguments = new object[] { permissions };
        }

        private class AuthorizePermissionFilter : IAsyncAuthorizationFilter
        {
            private readonly IPermissionService _permissionService;
            private readonly string[] _permissions;

            public AuthorizePermissionFilter(string[] permissions, IPermissionService permissionService)
            {
                _permissions = permissions;
                _permissionService = permissionService;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                foreach (var permission in _permissions)
                    if (await _permissionService.AuthorizeAsync(permission))
                        return;

                context.Result = new ChallengeResult();
            }
        }
    }
}

//StandardPermissionProvider.AccessAdminPanel