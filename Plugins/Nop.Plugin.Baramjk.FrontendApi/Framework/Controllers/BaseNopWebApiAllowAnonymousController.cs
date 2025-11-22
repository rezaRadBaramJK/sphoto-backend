using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.Baramjk.FrontendApi.Framework.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseNopWebApiAllowAnonymousController : ControllerBase
    {
    }
}