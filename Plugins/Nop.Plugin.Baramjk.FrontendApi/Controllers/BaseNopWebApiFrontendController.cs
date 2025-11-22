using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Controllers;

namespace Nop.Plugin.Baramjk.FrontendApi.Controllers
{
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [Area(WebApiFrontendDefaults.AREA)]
    [Route(WebApiFrontendDefaults.ROUTE, Order = int.MaxValue)]
    [ApiExplorerSettings(GroupName = WebApiFrontendDefaults.AREA)]
    public abstract class BaseNopWebApiFrontendController : BaseNopWebApiController
    {
    }

    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [Area(WebApiFrontendDefaults.AREA)]
    [Route(WebApiFrontendDefaults.ROUTE, Order = int.MaxValue)]
    [ApiExplorerSettings(GroupName = WebApiFrontendDefaults.AREA)]
    public abstract class BaseNopWebApiFrontendAllowAnonymousController : BaseNopWebApiAllowAnonymousController
    {
    }
}