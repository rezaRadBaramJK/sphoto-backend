using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes.Abstractions;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeContentAttribute: TypeFilterAttribute
    {
        public AuthorizeContentAttribute(params string[] permissions) 
            : base(typeof(AuthorizeContentAccessFilter))
        {
            Arguments = new object[] {  permissions };
        }


        private class AuthorizeContentAccessFilter: BaseAccessFilter
        {
            public AuthorizeContentAccessFilter(string[] permissions, IPermissionService permissionService) : base(permissions, permissionService)
            {
            }

            protected override Task<IActionResult> GetErrorResult(AuthorizationFilterContext context)
            {
                return Task.FromResult<IActionResult>(new ContentResult
                {
                    Content = "Access denied."
                });
            }
        }
    }
}