using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes.Abstractions;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeViewAccessAttribute : TypeFilterAttribute
    {
        public AuthorizeViewAccessAttribute(params string[] permissions)
        : base(typeof(AuthorizeViewAccessFilter))
        {
            Arguments = new object[] { permissions };
        }
        
        private class AuthorizeViewAccessFilter : BaseAccessFilter
        {
            public AuthorizeViewAccessFilter(
                string[] permissions,
                IPermissionService permissionService) 
                : base(permissions, permissionService)
            {
            }

            protected override Task<IActionResult> GetErrorResult(AuthorizationFilterContext context)
            {
                return Task.FromResult(AccessDeniedView(context.HttpContext.Request));
            }
        }
        
    }
}