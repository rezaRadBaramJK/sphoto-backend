using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Attributes.Abstractions
{
    public abstract class BaseAccessFilter : IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;
        private readonly IPermissionService _permissionService;
        
        protected BaseAccessFilter(string[] permissions, IPermissionService permissionService)
        {
            _permissions = permissions;
            _permissionService = permissionService;
        }

        protected abstract Task<IActionResult> GetErrorResult(AuthorizationFilterContext context);
        
        public virtual async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
                
            foreach (var permission in _permissions)
                if (await _permissionService.AuthorizeAsync(permission))
                    return;
                
            context.Result = await GetErrorResult(context);
        }
        
        protected static async Task<JsonResult> AccessDeniedDataTablesJson()
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

            return ErrorJson(await localizationService.GetResourceAsync("Admin.AccessDenied.Description"));
        }
        
        protected static IActionResult AccessDeniedView(HttpRequest request)
        {
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            return new RedirectToActionResult("AccessDenied", "Security", new { pageUrl = webHelper.GetRawUrl(request) });
        }
            
        protected static JsonResult ErrorJson(string error)
        {
            return new JsonResult(new
            {
                error
            });
        }
    }
    
    
}