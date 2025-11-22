using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes.Abstractions;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeApiAttribute : TypeFilterAttribute
    {
        public AuthorizeApiAttribute(params string[] permissions)
            : base(typeof(AuthorizeApiAccessFilter))
        {
            Arguments = new object[] { permissions };
        }

        private class AuthorizeApiAccessFilter : BaseAccessFilter
        {
            public AuthorizeApiAccessFilter(string[] permissions, IPermissionService permissionService) : base(permissions, permissionService)
            {
            }

            protected override Task<IActionResult> GetErrorResult(AuthorizationFilterContext context)
            {
                return Task.FromResult<IActionResult>(ApiResponseFactory.Unauthorized("Access denied."));
            }
        }
    }
}