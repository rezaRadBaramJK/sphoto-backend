using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.Banner.Attributes;
using Nop.Plugin.Baramjk.Framework.Models;
using Nop.Plugin.Baramjk.Framework.Mvc.Controller;

namespace Nop.Plugin.Baramjk.Banner.Controllers.Api.Backend.Abstractions
{
    [AdminAndVendorAuthorize]
    public abstract class BaseAdminVendorBackendApiController: BaseBaramjkApiController
    {
        public static IActionResult AccessDenied() => ApiResponseFactory.Unauthorized("Access denied.");
    }
}