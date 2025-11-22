using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Baramjk.Framework.Services.License;

namespace Nop.Plugin.Baramjk.Framework.Mvc.Filters
{
    public class LicenseAttribute : ActionFilterAttribute
    {
        private ILicenseService _licenseService;

        public string PluginName { get; set; }

        public bool IsAdminPage { get; set; } = true;

        public PageType PageType { get; set; } = PageType.AdminPage;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _licenseService = (ILicenseService)context.HttpContext.RequestServices.GetService(typeof(ILicenseService));
            if (_licenseService.IsLicensed(PluginName))
                return;

            context.HttpContext.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Result = new RedirectResult("/Admin/LicensePanel/List?pluginName=" + PluginName);
        }
    }

    public enum PageType
    {
        AdminPage,
        PublicPage,
        Api,
        Component
    }
}