using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Helpers;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Controllers
{
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public abstract class BaseNopWebApiController : ControllerBase
    {
    }
}