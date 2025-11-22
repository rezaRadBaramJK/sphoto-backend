using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Baramjk.PhotoPlatform.Attributes.Abstractions;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Areas.Admin.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeDataTableAttribute: TypeFilterAttribute
    {
        public AuthorizeDataTableAttribute(params string[] permissions) 
            : base(typeof(AuthorizeDataTableAccessFilter))
        {
            Arguments = new object[] {  permissions };
        }


        private class AuthorizeDataTableAccessFilter : BaseAccessFilter
        {
            public AuthorizeDataTableAccessFilter(
                string[] permissions,
                IPermissionService permissionService) 
                : base(permissions, permissionService)
            {
            }


            protected override async Task<IActionResult> GetErrorResult(AuthorizationFilterContext _)
            {
                return await AccessDeniedDataTablesJson();
            }
        }
    }
}